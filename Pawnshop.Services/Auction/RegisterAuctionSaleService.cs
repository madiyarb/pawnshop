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
    public class RegisterAuctionSaleService : IRegisterAuctionSaleService
    {
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IAuctionPaymentRepository _auctionPaymentRepository;
        private readonly GroupRepository _group;
        private readonly IRemittanceService _remittanceService;
        private readonly ICashOrderRemittanceRepository _orderRemittanceRepository;
        
        private readonly List<int> _cashOrderIds = new List<int>();

        public RegisterAuctionSaleService(
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
        
        public async Task RegisterAsync(RegisterAuctionSaleRequest request)
        {
            ValidateIncomingRequestModel(ref request);
            Group basKenseBranch = await _group.FindAsync(new { Name = Constants.BKS });

            var transaction = _auctionPaymentRepository.BeginTransaction();

            await RegisterCashOrders(request);
            var remittance = await CreateRemittance(request, basKenseBranch.Id, request.BranchId);
            await CreateAuctionPayments(request);
            await CreateCashOrderRemittance(remittance);
                
            transaction.Commit();
        }
        
        private async Task RegisterCashOrders(RegisterAuctionSaleRequest request)
        {
            Group branchTo = await _group.GetAsync(request.BranchId);
            if (branchTo is null)
            {
                throw new PawnshopApplicationException($"Филиал с Id: {request.BranchId} не найден!");
            }

            IDictionary<AmountType, decimal> amounts = new Dictionary<AmountType, decimal>();
            
            var profitAmount = GetAmountByProfit(request.Profit);
            if (profitAmount != null)
            {
                amounts.Add((KeyValuePair<AmountType, decimal>)profitAmount);
            }
            
            amounts.Add(new KeyValuePair<AmountType, decimal>(AmountType.Selling, request.Amount));
            
            if (request.Expenses.HasValue && request.Expenses > 0)
            {
                amounts.Add(new KeyValuePair<AmountType, decimal>(AmountType.Expense, (decimal)request.Expenses));
            }
            
            Group basKenseBranch = await _group.FindAsync(new { Name = Constants.BKS });
            var ordersWithRecords = await _businessOperationService.ExecuteRegistrationAsync(
                date: DateTime.Now,
                businessOperationCode: Constants.AUCTION_SALE_BUSINESS_OPERATION_CODE,
                branchFromId: basKenseBranch.Id,
                authorId: request.AuthorId,
                amounts: amounts,
                typeCode: Constants.AUCTION_HIERARCHY_TYPE_CODE,
                orderStatus: OrderStatus.WaitingForApprove,
                orderUserId: request.OrderUserId,
                note: request.Note,
                branchToId: branchTo.Id
                // clientId: request.ClientId
            );
            
            if (ordersWithRecords.IsNullOrEmpty())
            {
                throw new PawnshopApplicationException(
                    "При попытке регистрации БО по Реализации авто Аукциона не создались проводки!");
            }
            
            _cashOrderIds.AddRange(ordersWithRecords.Select(c => c.Item1.Id));
        }

        private KeyValuePair<AmountType, decimal>? GetAmountByProfit(decimal profit)
        {
            if (profit == 0) return null;
            if (profit > 0)
            {
                return new KeyValuePair<AmountType, decimal>(AmountType.SellingProfit, profit);
            }
            
            return new KeyValuePair<AmountType, decimal>(AmountType.SellingLoss, Math.Abs(profit));
        }
        
        private void ValidateIncomingRequestModel(ref RegisterAuctionSaleRequest model)
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

        private async Task CreateAuctionPayments(RegisterAuctionSaleRequest request)
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

        private async Task<Remittance> CreateRemittance(RegisterAuctionSaleRequest request, int branchFromId, int branchToId)
        {
            Remittance remittance = await _remittanceService.RegisterAsync(
                branchIdFrom: branchFromId,
                branchIdTo:branchToId,
                sum: request.Amount,
                note: request.Note ?? "Перевод по реализации по Аукциону",
                authorId: request.AuthorId);
            
            if (remittance.ReceiveOrderId.HasValue)
            {
                _cashOrderIds.Add((int)remittance.ReceiveOrderId);
            }
            
            return remittance;
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
    }
}