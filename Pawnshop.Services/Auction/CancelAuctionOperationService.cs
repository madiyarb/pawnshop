using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.Auction.Interfaces;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Auction.HttpServices.Interfaces;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.CashOrderRemittances;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.Remittances;
using Serilog;

namespace Pawnshop.Services.Auction
{
    public class CancelAuctionOperationService : ICancelAuctionOperationService
    {
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IAuctionPaymentRepository _auctionPaymentRepository;
        private readonly GroupRepository _group;
        private readonly IRemittanceService _remittanceService;
        private readonly IBusinessOperationSettingService _BOSService;
        private readonly ICashOrderRemittanceService _orderRemittanceService;
        private readonly ISessionContext _sessionContext;
        private readonly ICashOrderService _cashOrderService;
        private readonly IAuctionOperationHttpService _auctionOperationHttp;
        private readonly IUKassaService _uKassaService;
        private readonly ILogger _logger;

        public CancelAuctionOperationService(
            IBusinessOperationService businessOperationService,
            IAuctionPaymentRepository auctionPaymentRepository,
            GroupRepository group,
            IRemittanceService remittanceService,
            ICashOrderRemittanceService orderRemittanceService,
            IBusinessOperationSettingService bosService,
            ISessionContext sessionContext,
            ICashOrderService cashOrderService,
            IUKassaService uKassaService,
            IAuctionOperationHttpService auctionOperationHttp,
            ILogger logger)
        {
            _businessOperationService = businessOperationService;
            _auctionPaymentRepository = auctionPaymentRepository;
            _group = group;
            _remittanceService = remittanceService;
            _orderRemittanceService = orderRemittanceService;
            _BOSService = bosService;
            _sessionContext = sessionContext;
            _cashOrderService = cashOrderService;
            _uKassaService = uKassaService;
            _auctionOperationHttp = auctionOperationHttp;
            _logger = logger;
        }

        public async Task CancelAsync(CashOrder cashOrder)
        {
            CashOrder auctionExpenseCashOrder = await GetAuctionExpenseCashOrderAsync(cashOrder);
            var cashOrderRemittance = await _orderRemittanceService.GetByCashOrderId(cashOrder.Id);
            if (cashOrderRemittance is null) return;

            var remittance = await _remittanceService.GetAsync(cashOrderRemittance.RemittanceId);
            if (remittance is null) return;

            IDictionary<AmountType, decimal> amounts = new Dictionary<AmountType, decimal>();
            amounts.Add(new KeyValuePair<AmountType, decimal>(AmountType.Expense, auctionExpenseCashOrder.OrderCost));

            using IDbTransaction transaction = _auctionPaymentRepository.BeginTransaction();

            await _businessOperationService.ExecuteRegistrationAsync(
                date: DateTime.Now,
                businessOperationCode: Constants.AUCTION_CANCEL_EXPENSE_BUSINESS_OPERATION_CODE,
                branchFromId: auctionExpenseCashOrder.BranchId,
                authorId: _sessionContext.UserId,
                amounts: amounts,
                typeCode: Constants.AUCTION_HIERARCHY_TYPE_CODE,
                orderUserId: auctionExpenseCashOrder.UserId,
                note: Constants.AUCTION_CANCEL_WITHDRAW_EXPENSE_NOTE,
                orderStatus: OrderStatus.Approved
            );

            await _cashOrderService.DeleteCashOrderPrintLanguageForOrder(cashOrder);
            await _remittanceService.CancelAcceptAsync(remittance.Id, _sessionContext.UserId);

            transaction.Commit();

            _uKassaService.FinishRequests(new List<int> { cashOrder.Id });

            try
            {
                await _auctionOperationHttp.Reject(auctionExpenseCashOrder.AuctionRequestId.Value);
            }
            catch (PawnshopApplicationException e)
            {
                _logger.Error(e, "Ошибка при попытке отклонения аукциона после отмены операции");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Неизвестная ошибка при попытке отклонения аукциона после отмены операции");
            }
        }

        private async Task<CashOrder> GetAuctionExpenseCashOrderAsync(CashOrder cashOrder)
        {
            var auctionExpenseBOS = _BOSService.GetByCode(Constants.ACCOUNT_SETTING_EXPENSE);
            if (auctionExpenseBOS is null)
            {
                throw new PawnshopApplicationException($"Не найдена настройка бизнес операции с кодом: {Constants.ACCOUNT_SETTING_EXPENSE}");
            }

            if (cashOrder.BusinessOperationSettingId == auctionExpenseBOS.Id)
            {
                return cashOrder;
            }

            var auctionPayments = await _auctionPaymentRepository.GetByCashOrderIdAsync(cashOrder.Id);
            if (auctionPayments.IsNullOrEmpty())
            {
                throw new PawnshopApplicationException("Не найдены ордера, связанные с платежами аукциона");
            }

            var cashOrderIds = auctionPayments.Select(ap => ap.CashOrderId);
            var auctionCashOrders = await _cashOrderService.GetMultipleByIds(cashOrderIds);

            if (auctionCashOrders.IsNullOrEmpty())
            {
                throw new PawnshopApplicationException("Не найдены ордера, связанные с платежами аукциона");
            }

            var auctionExpenseCashOrder = auctionCashOrders.FirstOrDefault(co => co.BusinessOperationSettingId == auctionExpenseBOS.Id);
            if (auctionExpenseCashOrder == null)
            {
                throw new PawnshopApplicationException("Не найден ордер расхода для аукциона.");
            }

            return auctionExpenseCashOrder;
        }
    }
}