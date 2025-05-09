using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Investments;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Investment;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.InvestmentView)]
    public class InvestmentController : Controller
    {
        private readonly InvestmentRepository _investementRepository;
        private readonly CashOrderRepository _orderRepository;
        private readonly CashOrderNumberCounterRepository _counterRepository;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;

        public InvestmentController(InvestmentRepository investmentRepository, 
                                    CashOrderRepository orderRepository,
                                    CashOrderNumberCounterRepository counterRepository,
                                    BranchContext branchContext, 
                                    ISessionContext sessionContext)
        {
            _investementRepository = investmentRepository;
            _orderRepository = orderRepository;
            _counterRepository = counterRepository;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<Investment> List([FromBody] ListQueryModel<InvestmentListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<InvestmentListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new InvestmentListQueryModel();
            listQuery.Model.OwnerId = _branchContext.Branch.Id;

            return new ListModel<Investment>
            {
                List = _investementRepository.List(listQuery, listQuery.Model),
                Count = _investementRepository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public Investment Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _investementRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.InvestmentManage)]
        public Investment Save([FromBody] Investment model)
        {
            if (model.Id == 0)
            {
                model.CreateDate = DateTime.Now;
                model.BranchId = _branchContext.Branch.Id;
                model.AuthorId = _sessionContext.UserId;
                model.OwnerId = _branchContext.Branch.Id;
            }

            model.InvestmentEndDate = model.InvestmentBeginDate.AddDays(model.InvestmentPeriod);
            model.ActualCost = model.InvestmentCost;
            model.ActualPeriod = model.InvestmentPeriod;
            model.ActualEndDate = model.InvestmentEndDate;

            ModelState.Clear();
            TryValidateModel(model);
            ModelState.Validate();

            if (model.Id > 0)
            {
                var current = _investementRepository.Get(model.Id);
                if (current.Status > InvestmentStatus.Draft)
                {
                    throw new PawnshopApplicationException("Внесение изменений в подписанный договор невозможно");
                }

                _investementRepository.Update(model);
            }
            else
            {
                _investementRepository.Insert(model);
            }

            return model;
        }

        [HttpPost, Authorize(Permissions.InvestmentManage)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _investementRepository.Get(id);
            if (model == null) throw new InvalidOperationException();
            if (model.Status > InvestmentStatus.Draft) throw new PawnshopApplicationException("Внесение изменений в подписанный договор невозможно");

            _investementRepository.Delete(id);
            return Ok();
        }

        public IActionResult Sign([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _investementRepository.Get(id);
            if (model == null) throw new InvalidOperationException();
            if (model.Status > InvestmentStatus.Draft) throw new PawnshopApplicationException("Договор уже подписан");

            using (var transaction = _investementRepository.BeginTransaction())
            {
                var order = new CashOrder
                {
                    OrderType = OrderType.CashIn,
                    ClientId = model.ClientId,
                    DebitAccountId = 0, //TODO: В настройках добавить дебет и кредит для инвестиций
                    CreditAccountId = 0,
                    OrderCost = model.InvestmentCost,
                    OrderDate = model.InvestmentBeginDate,
                    RegDate = DateTime.Now,
                    OwnerId = _branchContext.Branch.Id,
                    BranchId = _branchContext.Branch.Id,
                    AuthorId = _sessionContext.UserId,
                    OrderNumber = _counterRepository.Next(
                        OrderType.CashIn, 
                        model.InvestmentBeginDate.Year, 
                        _branchContext.Branch.Id, 
                        _branchContext.Configuration.CashOrderSettings.CashInNumberCode)
                };

                _orderRepository.Insert(order);

                model.OrderId = order.Id;
                model.Status = InvestmentStatus.Signed;

                _investementRepository.Update(model);

                transaction.Commit();
            }

            return Ok();
        }
    }
}
