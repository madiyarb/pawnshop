using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Insurance;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.InsuranceView)]
    public class InsuranceController : Controller
    {
        private readonly InsuranceRepository _insuranceRepository;
        private readonly ContractRepository _contractRepository;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;

        public InsuranceController(InsuranceRepository insuranceRepository, ContractRepository contractRepository, BranchContext branchContext, ISessionContext sessionContext)
        {
            _insuranceRepository = insuranceRepository;
            _contractRepository = contractRepository;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<Insurance> List([FromBody] ListQueryModel<InsuranceListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<InsuranceListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new InsuranceListQueryModel();
            listQuery.Model.OwnerId = _branchContext.Branch.Id;

            if (listQuery.Model.EndDate.HasValue)
            {
                listQuery.Model.EndDate = listQuery.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return new ListModel<Insurance>
            {
                List = _insuranceRepository.List(listQuery, listQuery.Model),
                Count = _insuranceRepository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public Insurance Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _insuranceRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost]
        public Insurance Find([FromBody] InsuranceQueryModel query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (query.ContractId <= 0) throw new ArgumentOutOfRangeException(nameof(query.ContractId));

            var model = _insuranceRepository.Find(query);
            if (model == null)
            {
                var contract = _contractRepository.Get(query.ContractId);
                model = new Insurance
                {
                    ContractId = query.ContractId,
                    Contract = contract,
                    InsurancePeriod = 365,
                    BeginDate = contract.ContractDate.AddDays(1),
                    BranchId = _branchContext.Branch.Id,
                    UserId = _sessionContext.UserId,
                    OwnerId = _branchContext.Branch.Id
                };
            }

            return model;
        }

        [HttpPost, Authorize(Permissions.InsuranceManage)]
        public Insurance Save([FromBody] Insurance model)
        {
            if (model.Id == 0)
            {
                model.OwnerId = _branchContext.Branch.Id;
                model.BranchId = _branchContext.Branch.Id;
                model.UserId = _sessionContext.UserId;
            }

            ModelState.Clear();
            TryValidateModel(model);
            ModelState.Validate();

            if (model.Id > 0)
            {
                _insuranceRepository.Update(model);
            }
            else
            {
                _insuranceRepository.Insert(model);
            }

            return model;
        }

        [HttpPost, Authorize(Permissions.InsuranceManage)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _insuranceRepository.Get(id);
            if (model == null) throw new InvalidOperationException();
            if (model.Status > InsuranceStatus.Draft) throw new PawnshopApplicationException("Запрещено удалять подписанные страховые договоры");

            _insuranceRepository.Delete(id);
            return Ok();
        }
    }
}
