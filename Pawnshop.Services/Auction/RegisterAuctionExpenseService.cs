
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.Auction.Interfaces;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;
using Pawnshop.Data.Models.Auction;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Remittances;

namespace Pawnshop.Services.Auction
{
    public class RegisterAuctionExpenseService : IRegisterAuctionExpenseService
    {
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IAuctionPaymentRepository _auctionPaymentRepository;
        private readonly GroupRepository _group;
        private readonly IRemittanceService _remittanceService;
        private readonly ICashOrderRemittanceRepository _orderRemittanceRepository;

        private readonly List<int> _cashOrderIds = new List<int>();

        public RegisterAuctionExpenseService(
            IBusinessOperationService businessOperationService,
            IAuctionPaymentRepository auctionPaymentRepository,
            GroupRepository group,
            IRemittanceService remittanceService,
            ICashOrderRemittanceRepository orderRemittanceRepository)
        {
            _businessOperationService = businessOperationService;
            _auctionPaymentRepository = auctionPaymentRepository;
            _group = group;
            _remittanceService = remittanceService;
            _orderRemittanceRepository = orderRemittanceRepository;
        }

        public async Task RegisterAsync(RegisterAuctionExpenseRequest request)
        {
            try
            {
                ValidateIncomingRequestModel(ref request);
                Group basKenseBranch = await _group.FindAsync(new { Name = Constants.BKS });

                var transaction = _auctionPaymentRepository.BeginTransaction();

                await RegisterCashOrder(request, basKenseBranch);
                var remittance = await CreateRemittance(request, request.BranchId, basKenseBranch.Id);
                await CreateAuctionPayments(request);
                await CreateCashOrderRemittance(remittance);

                transaction.Commit();
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Произошла ошибка: {e.Message}", e);
            }
        }

        private async Task CreateCashOrderRemittance(Remittance remittance)
        {
            foreach (var cashOrderId in _cashOrderIds)
            {
                await _orderRemittanceRepository.Insert(new CashOrderRemittance
                {
                    CashOrderId = cashOrderId,
                    RemittanceId = remittance.Id
                });
            }
        }

        private void ValidateIncomingRequestModel(ref RegisterAuctionExpenseRequest model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Модель данных не может быть пустой");

            if (model.RequestId == Guid.Empty)
                throw new ArgumentException("Идентификатор запроса не может быть пустым", nameof(model.RequestId));

            if (model.CreateDate == default(DateTime))
                throw new ArgumentException("Дата создания должна быть задана", nameof(model.CreateDate));

            if (model.BranchId <= 0)
                throw new ArgumentException("Идентификатор филиала должен быть положительным числом",
                    nameof(model.BranchId));

            if (model.AuthorId <= 0)
                throw new ArgumentException("Идентификатор автора должен быть положительным числом",
                    nameof(model.AuthorId));

            if (model.Amount <= 0)
                throw new ArgumentException("Сумма должна быть положительным числом", nameof(model.Amount));
        }

        private async Task RegisterCashOrder(RegisterAuctionExpenseRequest request, Group branchFrom)
        {
            IDictionary<AmountType, decimal> amounts = new Dictionary<AmountType, decimal>();
            amounts.Add(new KeyValuePair<AmountType, decimal>(AmountType.Expense, request.Amount));

            var ordersWithRecords = await _businessOperationService.ExecuteRegistrationAsync(
                date: DateTime.Now,
                businessOperationCode: Constants.AUCTION_EXPENSE_BUSINESS_OPERATION_CODE,
                branchFromId: branchFrom.Id,
                authorId: request.AuthorId,
                amounts: amounts,
                typeCode: Constants.AUCTION_HIERARCHY_TYPE_CODE,
                orderUserId: request.OrderUserId,
                branchToId: request.BranchId,
                note: request.Note ?? "Создание расхода по авто (списание на расходы CarTAS). Обнуляется касса Бас Кенсе"
            );

            if (ordersWithRecords.IsNullOrEmpty())
            {
                throw new PawnshopApplicationException(
                    "При попытке регистрации БО по расходу для авто Аукциона не создались проводки!");
            }

            _cashOrderIds.AddRange(ordersWithRecords.Select(c => c.Item1.Id));
        }

        private async Task<Remittance> CreateRemittance(RegisterAuctionExpenseRequest request, int branchFromId, int branchToId)
        {
            Remittance remittance = await _remittanceService.RegisterAsync(
                branchIdFrom: branchFromId,
                branchIdTo: branchToId,
                sum: request.Amount,
                note: request.Note ?? "Расход для авто по Аукциону",
                authorId: request.AuthorId);

            if (remittance.ReceiveOrderId.HasValue)
            {
                _cashOrderIds.Add((int)remittance.ReceiveOrderId);
            }
            
            return remittance;
        }

        private async Task CreateAuctionPayments(RegisterAuctionExpenseRequest request)
        {
            if (_cashOrderIds.IsNullOrEmpty()) return;

            var auctionPayments = _cashOrderIds.Select(coId =>
                new AuctionPayment
                {
                    RequestId = request.RequestId,
                    AuthorId = request.AuthorId,
                    CashOrderId = coId,
                    CreateDate = DateTime.Now
                }).ToList();

            await _auctionPaymentRepository.InsertMultipleAsync(auctionPayments);
        }
    }
}