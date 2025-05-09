using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.UKassa;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.UKassa
{
    public class UKassaReportsService : IUKassaReportsService
    {
        private readonly IUKassaHttpService _uKassaHttpService;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly AccountRepository _accountRepository;
        private readonly UKassaAccountSettingsRepository _uKassaAccountSettingsRepository;
        private readonly UKassaRepository _uKassaRepository;
        private readonly UserRepository _userRepository;

        public UKassaReportsService(IUKassaHttpService uKassaHttpService, CashOrderRepository cashOrderRepository, AccountRepository accountRepository,
            UKassaAccountSettingsRepository uKassaAccountSettingsRepository, UKassaRepository uKassaRepository, UserRepository userRepository)
        {
            _uKassaHttpService = uKassaHttpService;
            _cashOrderRepository = cashOrderRepository;
            _accountRepository = accountRepository;
            _uKassaAccountSettingsRepository = uKassaAccountSettingsRepository;
            _uKassaRepository = uKassaRepository;
            _userRepository = userRepository;
        }

        public async Task<List<UKassaReconciliationReport>> GetReport(int branchId, int? shiftId, DateTime date)
        {
            var reconciliationReportList = new List<UKassaReconciliationReport>();
            var account = _accountRepository.GetBranchMainAccount(branchId);
            var ukassaAccount = _uKassaAccountSettingsRepository.GetByAccountId(account.Id);
            if (ukassaAccount == null)
                throw new PawnshopApplicationException($"Для этого отделения не найдены настройки UKassa");

            var report = new UKassaReportResponse();
            if (date.Date == DateTime.Now.Date)
                report = _uKassaHttpService.GetXReport(ukassaAccount.KassaId);
            else if (date.Date < DateTime.Now.Date && shiftId.HasValue)
                report = _uKassaHttpService.GetZReport(shiftId.Value);
            var ukassaReport = new UKassaReconciliationReport();
            ukassaReport.ReconciliationMoneyPlacements = new List<ReconciliationMoneyPlacement>();
            ukassaReport.ReconciliationCheckOperations = new List<ReconciliationCheckOperation>();
            ukassaReport.Side = "ukassa";
            if (report.duplicated != null)
            {
                if (report.duplicated.money_placements != null)
                {
                    report.duplicated.money_placements.ForEach(x => ukassaReport.ReconciliationMoneyPlacements.Add(new ReconciliationMoneyPlacement()
                    {
                        OperationType = x.operation,
                        OperationsCount = x.operations_count,
                        MoneyPlacementSum = Convert.ToDecimal(x.operations_sum.bills) + Convert.ToDecimal(x.operations_sum.coins) / 100
                    }));
                }
                if (report.duplicated.ticket_operations != null)
                {
                    foreach (var ticketOperation in report.duplicated.ticket_operations)
                    {
                        var operation = new ReconciliationCheckOperation()
                        {
                            OperationType = ticketOperation.operation,
                            OperationsCount = ticketOperation.tickets_count,
                            OperationSum = Convert.ToDecimal(ticketOperation.tickets_sum.bills) + Convert.ToDecimal(ticketOperation.tickets_sum.coins) / 100
                        };
                        var reconciliationPayments = new List<ReconciliationPayment>();
                        if (ticketOperation.payments != null)
                            foreach (var payment in ticketOperation.payments)
                            {
                                reconciliationPayments.Add(new ReconciliationPayment()
                                {
                                    PaymentType = payment.payment,
                                    PaymentSum = Convert.ToDecimal(payment.sum.bills) + Convert.ToDecimal(payment.sum.coins) / 100
                                });
                            };
                        operation.ReconciliationPayments = reconciliationPayments;
                        ukassaReport.ReconciliationCheckOperations.Add(operation);
                    }
                }
                ukassaReport.Cash = Convert.ToDecimal(report.duplicated.cash_sum.bills) + Convert.ToDecimal(report.duplicated.cash_sum.coins) / 100;
            }
            reconciliationReportList.Add(ukassaReport);

            var ourReport = new UKassaReconciliationReport();
            ourReport.ReconciliationMoneyPlacements = new List<ReconciliationMoneyPlacement>();
            ourReport.ReconciliationCheckOperations = new List<ReconciliationCheckOperation>();
            ourReport.Side = "frontbaza";
            var moneyPlacementReport = _cashOrderRepository.GetMoneyPlacementsReport(branchId, date.Date, date.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            moneyPlacementReport.ForEach(x => ourReport.ReconciliationMoneyPlacements.Add(new ReconciliationMoneyPlacement()
            {
                OperationType = x.OperationType,
                OperationsCount = x.OperationsCount,
                MoneyPlacementSum = x.MoneyPlacementSum
            }));
            var cashOrderReportRows = _cashOrderRepository.GetCashOrdersReport(branchId, date.Date, date.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            foreach (var cashOrderReportRow in cashOrderReportRows)
            {
                var operation = new ReconciliationCheckOperation()
                {
                    OperationType = cashOrderReportRow.OperationType,
                    OperationsCount = cashOrderReportRow.OperationsCount,
                    OperationSum = cashOrderReportRow.OperationSum
                };
                var reconciliationPayments = new List<ReconciliationPayment>();
                if (cashOrderReportRow.CashOrdersPayments != null)
                    foreach (var payment in cashOrderReportRow.CashOrdersPayments)
                    {
                        reconciliationPayments.Add(new ReconciliationPayment()
                        {
                            PaymentType = payment.PaymentType,
                            PaymentSum = payment.PaymentSum
                        });
                    };
                operation.ReconciliationPayments = reconciliationPayments;
                ourReport.ReconciliationCheckOperations.Add(operation);
            }
            ourReport.Cash = _accountRepository.GetAccountBalance(ukassaAccount.AccountId, date);
            var damuAccount = _accountRepository.GetBranchDamuAccount(branchId);
            if (damuAccount != null)
            {
                var damuCash = _accountRepository.GetAccountBalance(damuAccount.Id, date);
                ourReport.Cash += damuCash;
            }
            reconciliationReportList.Add(ourReport);
            return reconciliationReportList;
        }

        public async Task<ListModel<CashOrderUKassaReportDto>> GetOperations(int branchId, int shiftId, DateTime date, Page page, string filter, int? status)
        {
            var reportList = new List<CashOrderUKassaReportDto>();
            var ukassaOperations = _uKassaHttpService.GetShiftOperations(shiftId);
            var cashOrdersList = _cashOrderRepository.GetOnlyCashOrdersByBranchAndDate(branchId, date.Date, date.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

            foreach (var order in cashOrdersList)
            {
                string authorName = "";
                if(order.ApprovedId.HasValue)
                    authorName = _userRepository.Get(order.ApprovedId.Value)?.Fullname;

                var dto = new CashOrderUKassaReportDto()
                {
                    OrderId = order.Id,
                    OrderType = order.OrderType,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    OrderCost = order.OrderCost,
                    DebitAccount = order.DebitAccount?.Code,
                    CreditAccount = order.CreditAccount?.Code,
                    AuthorName = authorName,
                    Reason = order.Reason
                };
                reportList.Add(dto);
                var ukassaRequest = _uKassaRepository.GetByOrderId(order.Id);
                if (ukassaRequest == null)
                {
                    dto.Status = CashOrderUKassaStatusEnum.NotFoundInUKassa;
                    continue;
                }
                if (ukassaRequest.Status == Data.Models.TasOnline.TasOnlineRequestStatus.New ||
                    ukassaRequest.Status == Data.Models.TasOnline.TasOnlineRequestStatus.Error)
                {
                    dto.Status = CashOrderUKassaStatusEnum.NotFoundInUKassa;
                    continue;
                }
                if (ukassaRequest.TotalAmount != order.OrderCost)
                {
                    dto.Status = CashOrderUKassaStatusEnum.AmountsNotEqual;
                    continue;
                }
                dto.RequestId = ukassaRequest.Id;
                dto.CheckNumber = ukassaRequest.ResponseCheckNumber;
            }

            foreach (var dto in reportList.Where(x => x.CheckNumber != null))
            {
                if(!ukassaOperations.Any(x => x.fixed_check == dto.CheckNumber))
                {
                    dto.Status = CashOrderUKassaStatusEnum.NotFoundInUKassa;
                    continue;
                }
                var op = ukassaOperations.FirstOrDefault(x => x.fixed_check == dto.CheckNumber);
                if(op.total_amount != dto.OrderCost)
                {
                    dto.Status = CashOrderUKassaStatusEnum.AmountsNotEqual;
                    continue;
                }
            }

            if (string.IsNullOrEmpty(filter))
            {
                foreach (var item in ukassaOperations)
                {
                    if (!reportList.Any(x => x.CheckNumber == item.fixed_check))
                    {
                        var dto = new CashOrderUKassaReportDto()
                        {
                            CheckNumber = item.fixed_check,
                            OrderDate = DateTime.ParseExact(item.created_at, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                            OrderCost = item.total_amount,
                            Status = CashOrderUKassaStatusEnum.NotFoundInFrontBase
                        };
                        reportList.Add(dto);
                    }
                }
            }

            foreach (var dto in reportList.Where(x => x.Status == 0))
                dto.Status = CashOrderUKassaStatusEnum.OK;

            if(status.HasValue)
            {
                var st = (CashOrderUKassaStatusEnum)status.Value;
                reportList = reportList.Where(x => x.Status == st).ToList();
            }

            var returnModel = new ListModel<CashOrderUKassaReportDto>();
            returnModel.List = reportList.OrderByDescending(x => x.OrderId).Skip(page.Offset).Take(page.Limit).ToList();
            returnModel.Count = reportList.Count;
            return returnModel;
        }

        public async Task<List<Shift>> GetShifts(int branchId, DateTime date)
        {
            var account = _accountRepository.GetBranchMainAccount(branchId);
            var ukassaAccount = _uKassaAccountSettingsRepository.GetByAccountId(account.Id);
            var shiftList = new List<Shift>();
            if (ukassaAccount == null)
                throw new PawnshopApplicationException($"Для этого отделения не найдены настройки UKassa");
            if (date.Date == DateTime.Now.Date)
                shiftList.Add(_uKassaHttpService.GetActiveShift(ukassaAccount.KassaId));
            else
                shiftList = _uKassaHttpService.GetShiftReports(ukassaAccount.KassaId, date.Date, date.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            return shiftList;
        }
    }
}
