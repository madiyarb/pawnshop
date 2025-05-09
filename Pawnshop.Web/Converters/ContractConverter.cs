using Pawnshop.Data.Models.AccountRecords;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Models.AbsOnline;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Data.Models.CreditLines;

namespace Pawnshop.Web.Converters
{
    public static class ContractConverter
    {
        public static ContractViewModel ToShortOnlineViewModel(
            this Contract contract,
            Car car,
            bool hasPartialPayment,
            Data.Models.Contracts.ContractBalance balance,
            List<ContractPaymentSchedule> paymentsSchedule,
            decimal nextPaymentAmount = 0,
            bool isCollecting = false
            )
        {
            var paymentExpiredDays = 0;
            var currentPayment = paymentsSchedule?.OrderBy(x => x.Date)
                .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date > DateTime.Now);

            var viewModel = new ContractViewModel
            {
                ContractNumber = contract.ContractNumber,
                DateOpen = contract.ContractDate.Date,
                DateClose = contract.MaturityDate.Date,
                LoanCost = contract.LoanCost,
                Car = car != null ? $"{car.Mark} {car.Model}" : string.Empty,
                CarNumber = car?.TransportNumber,
                Percent = Math.Round(contract.LoanPercent * 30, 2),
                HasPartialPayment = hasPartialPayment,
                ContractType = GetContractTypeName(contract.ContractClass),
                PrincipalDebt = balance?.AccountAmount + balance?.OverdueAccountAmount,
                Profit = balance?.ProfitAmount + balance?.OverdueProfitAmount,
                Penalty = balance?.PenyAmount,
                NextPaymentDate = contract.NextPaymentDate?.Date ?? new DateTime(1, 1, 1),
                AccountBalance = balance?.PrepaymentBalance ?? 0,
                Balance = balance?.PrepaymentBalance ?? 0,
                PaidPaymentsCount = paymentsSchedule.Count(x => x.ActionId.HasValue && x.ActualDate.HasValue),
                ExpiredPaymentsCount = paymentsSchedule.Count(x => x.Date.Date < DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue),
                PaymentsCount = paymentsSchedule.Count(),
                ProductCode = contract.Setting?.Id.ToString(),
                ProductName = contract.Setting?.Name,
            };

            if (viewModel.NextPaymentDate < DateTime.Now.Date)
                paymentExpiredDays = Math.Abs((viewModel.NextPaymentDate.Date - DateTime.Now.Date).Days);

            // костыль чтобы в мобильном приложении для закрытых займов не показывал график
            if ((int)contract.Status > 30)
            {
                viewModel.NextPaymentDate = new DateTime(1, 1, 1);
                return viewModel;
            }

            if (nextPaymentAmount > 0 && (paymentExpiredDays > 0 || viewModel.NextPaymentDate == DateTime.Now.Date))
            {
                viewModel.NextPaymentDate = DateTime.Now.Date;
                viewModel.NextPaymentAmount = nextPaymentAmount;
            }
            else
            {
                viewModel.NextPaymentAmount = currentPayment == null ? 0 : currentPayment.DebtCost + currentPayment.PercentCost + (balance?.PenyAmount ?? 0);
            }

            if (isCollecting)
            {
                viewModel.RepaymentAmount = viewModel.PrincipalDebt + viewModel.Profit + viewModel.Penalty;
                viewModel.OverdueDebt = balance?.OverdueAccountAmount + balance?.OverdueProfitAmount + balance?.PenyAmount;
                viewModel.PaymentExpiredDays = paymentExpiredDays;
            }

            return viewModel;
        }

        public static ContractViewModel ToShortOnlineViewModel(
            this Contract contract,
            Car car,
            bool hasPartialPayment,
            CreditLineBalance balance,
            List<ContractPaymentSchedule> paymentsSchedule,
            decimal nextPaymentAmount = 0,
            bool isCollecting = false
        )
        {
            var paymentExpiredDays = 0;
            var currentPayment = paymentsSchedule?.OrderBy(x => x.Date)
                .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date > DateTime.Now);

            var viewModel = new ContractViewModel
            {
                ContractNumber = contract.ContractNumber,
                DateOpen = contract.ContractDate.Date,
                DateClose = contract.MaturityDate.Date,
                LoanCost = contract.LoanCost,
                Car = car != null ? $"{car.Mark} {car.Model}" : string.Empty,
                CarNumber = car?.TransportNumber,
                Percent = Math.Round(contract.LoanPercent * 30, 2),
                HasPartialPayment = hasPartialPayment,
                ContractType = GetContractTypeName(contract.ContractClass),
                PrincipalDebt = balance?.SummaryAccountAmount + balance?.SummaryOverdueAccountAmount,
                Profit = balance?.SummaryProfitAmount + balance?.SummaryOverdueProfitAmount,
                Penalty = balance?.SummaryPenyAmount,
                NextPaymentDate = contract.NextPaymentDate?.Date ?? new DateTime(1, 1, 1),
                AccountBalance = balance?.SummaryPrepaymentBalance ?? 0,
                Balance = balance?.SummaryPrepaymentBalance ?? 0,
                PaidPaymentsCount = paymentsSchedule.Count(x => x.ActionId.HasValue && x.ActualDate.HasValue),
                ExpiredPaymentsCount = paymentsSchedule.Count(x => x.Date.Date < DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue),
                PaymentsCount = paymentsSchedule.Count(),
                ProductCode = contract.Setting?.Id.ToString(),
                ProductName = contract.Setting?.Name,
            };

            if (viewModel.NextPaymentDate < DateTime.Now.Date)
                paymentExpiredDays = Math.Abs((viewModel.NextPaymentDate.Date - DateTime.Now.Date).Days);

            // костыль чтобы в мобильном приложении для закрытых займов не показывал график
            if ((int)contract.Status > 30)
            {
                viewModel.NextPaymentDate = new DateTime(1, 1, 1);
                return viewModel;
            }

            if (nextPaymentAmount > 0 && (paymentExpiredDays > 0 || viewModel.NextPaymentDate == DateTime.Now.Date))
            {
                viewModel.NextPaymentDate = DateTime.Now.Date;
                viewModel.NextPaymentAmount = nextPaymentAmount;
            }
            else
            {
                viewModel.NextPaymentAmount = currentPayment == null ? 0 : currentPayment.DebtCost + currentPayment.PercentCost + (balance?.SummaryPenyAmount ?? 0);
            }

            if (isCollecting)
            {
                viewModel.RepaymentAmount = viewModel.PrincipalDebt + viewModel.Profit + viewModel.Penalty;
                viewModel.OverdueDebt = balance?.SummaryOverdueAccountAmount + balance?.SummaryOverdueProfitAmount + balance?.SummaryPenyAmount;
                viewModel.PaymentExpiredDays = paymentExpiredDays;
            }

            return viewModel;
        }

        public static ContractViewModel ToOnlineViewModel(
            this Contract contract,
            Car car,
            bool hasPartialPayment,
            Data.Models.Contracts.ContractBalance balance,
            List<ContractPaymentSchedule> paymentsSchedule,
            decimal nextPaymentAmount = 0,
            bool isCollecting = false,
            List<MovementsOfDepoAccount> paidPayments = null
            )
        {
            var viewModel = ToShortOnlineViewModel(contract, car, hasPartialPayment, balance, paymentsSchedule, nextPaymentAmount, isCollecting);

            // костыль чтобы в мобильном приложении для закрытых займов не показывал график
            if ((int)contract.Status > 30)
                return viewModel;

            viewModel.DebtCurrent = balance?.CurrentDebt ?? 0;

            viewModel.OverdueAndUpcomingPayments = paymentsSchedule
                .Where(x => x.Date.Date < DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue)
                .Select(x => new PaymentScheduleViewModel
                {
                    Date = x.Date,
                    Amount = x.DebtCost + x.PercentCost,
                    PrincipalDebt = x.DebtCost,
                    Percent = viewModel.Percent,
                    ProfitAmount = x.PercentCost,
                    Status = "Просроченный",
                    FineAmount = balance?.PenyAmount
                })
                .ToList();

            viewModel.OverdueAndUpcomingPayments.AddRange(paymentsSchedule
                .Where(x => x.Date.Date >= DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue)
                .Select(x => new PaymentScheduleViewModel
                {
                    Date = x.Date,
                    Amount = x.DebtCost + x.PercentCost,
                    PrincipalDebt = x.DebtCost,
                    Percent = viewModel.Percent,
                    ProfitAmount = x.PercentCost,
                    Status = "Предстоящий"
                }));

            if (isCollecting)
            {
                viewModel.RepaidPayments = paidPayments?
                    .Select(x => new PaymentScheduleViewModel
                    {
                        Date = x.Date,
                        Amount = x.TotalAcountAmount + x.TotalProfitAmount + x.PenyAmount,
                        PrincipalDebt = x.TotalAcountAmount,
                        Percent = viewModel.Percent,
                        ProfitAmount = x.TotalProfitAmount,
                        PenaltyAmount = x.PenyAmount,
                        FineAmount = 0
                    })
                    .ToList();
            }
            else
            {
                viewModel.RepaidPayments = paymentsSchedule
                    .Where(x => x.ActionId.HasValue && x.ActualDate.HasValue)
                    .Select(x => new PaymentScheduleViewModel
                    {
                        Date = x.Date,
                        Amount = x.DebtCost + x.PercentCost + (x.PenaltyCost ?? 0),
                        PrincipalDebt = x.DebtCost,
                        Percent = viewModel.Percent,
                        ProfitAmount = x.PercentCost,
                        PenaltyAmount = x.PenaltyCost,
                        FineAmount = 0
                    })
                    .ToList();
            }

            return viewModel;
        }


        public static ContractViewModel ToOnlineViewModel(
        this Contract contract,
        Car car,
        bool hasPartialPayment,
        CreditLineBalance balance,
        List<ContractPaymentSchedule> paymentsSchedule,
        decimal nextPaymentAmount = 0,
        bool isCollecting = false,
        List<MovementsOfDepoAccount> paidPayments = null)
        {
            var viewModel = ToShortOnlineViewModel(contract, car, hasPartialPayment, balance, paymentsSchedule, nextPaymentAmount, isCollecting);

            // костыль чтобы в мобильном приложении для закрытых займов не показывал график
            if ((int)contract.Status > 30)
                return viewModel;

            viewModel.DebtCurrent = balance?.SummaryCurrentDebt ?? 0;

            viewModel.OverdueAndUpcomingPayments = paymentsSchedule
                .Where(x => x.Date.Date < DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue)
                .Select(x => new PaymentScheduleViewModel
                {
                    Date = x.Date,
                    Amount = x.DebtCost + x.PercentCost,
                    PrincipalDebt = x.DebtCost,
                    Percent = viewModel.Percent,
                    ProfitAmount = x.PercentCost,
                    Status = "Просроченный",
                    FineAmount = balance?.SummaryPenyAmount
                })
                .ToList();

            viewModel.OverdueAndUpcomingPayments.AddRange(paymentsSchedule
                .Where(x => x.Date.Date >= DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue)
                .Select(x => new PaymentScheduleViewModel
                {
                    Date = x.Date,
                    Amount = x.DebtCost + x.PercentCost,
                    PrincipalDebt = x.DebtCost,
                    Percent = viewModel.Percent,
                    ProfitAmount = x.PercentCost,
                    Status = "Предстоящий"
                }));

            if (isCollecting)
            {
                viewModel.RepaidPayments = paidPayments?
                    .Select(x => new PaymentScheduleViewModel
                    {
                        Date = x.Date,
                        Amount = x.TotalAcountAmount + x.TotalProfitAmount + x.PenyAmount,
                        PrincipalDebt = x.TotalAcountAmount,
                        Percent = viewModel.Percent,
                        ProfitAmount = x.TotalProfitAmount,
                        PenaltyAmount = x.PenyAmount,
                        FineAmount = 0
                    })
                    .ToList();
            }
            else
            {
                viewModel.RepaidPayments = paymentsSchedule
                    .Where(x => x.ActionId.HasValue && x.ActualDate.HasValue)
                    .Select(x => new PaymentScheduleViewModel
                    {
                        Date = x.Date,
                        Amount = x.DebtCost + x.PercentCost + (x.PenaltyCost ?? 0),
                        PrincipalDebt = x.DebtCost,
                        Percent = viewModel.Percent,
                        ProfitAmount = x.PercentCost,
                        PenaltyAmount = x.PenaltyCost,
                        FineAmount = 0
                    })
                    .ToList();
            }

            return viewModel;
        }

        public static CreditLineViewModel ToCreditLineOnlineViewModel(
            this Contract contract,
            Car car,
            decimal remainingAmount
            )
        {
            return new CreditLineViewModel
            {
                ContractNumber = contract.ContractNumber,
                EndDate = contract.MaturityDate.Date,
                RemainingAmount = Math.Truncate(remainingAmount),
                CarNumber = car.TransportNumber,
                CarVin = car.BodyNumber,
            };
        }


        private static string GetContractTypeName(ContractClass contractClass)
        {
            switch (contractClass)
            {
                case ContractClass.Credit:
                    return "simple";
                case ContractClass.CreditLine:
                    return "credit_line";
                case ContractClass.Tranche:
                    return "tranche";
                default:
                    return "simple";
            }
        }
    }
}
