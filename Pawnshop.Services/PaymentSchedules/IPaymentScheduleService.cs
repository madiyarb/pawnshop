using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.PaymentSchedules
{
    public interface IPaymentScheduleService
    {
        List<ContractPaymentSchedule> Build(ScheduleType scheduleType, decimal loanCost, decimal percentPerDay, DateTime beginDate, DateTime maturityDate,
            DateTime? firstPaymentDate = null, bool isPartialPayment = false, bool isMigrated = false, bool isChangeControlDate = false, int? upcomingPaymentsCount = null, bool isRestructuring = false);

        void BuildForChangeControlDate(Contract contract, DateTime newControlDate);

        void BuildForPartialPayment(Contract contract, DateTime startDate, DateTime firstPaymentDate, decimal leftLoanCost,
            decimal amortizedBalanceOfDefferedPercent = 0, decimal amortizedBalanceOfOverduePercent = 0, decimal amortizedPenaltyOfOverdueDebt = 0, decimal amortizedPenaltyOfOverduePercent = 0);

        void BuildWithContract(Contract contract);

        void CheckPayDay(int day);

        Task CheckPayDayFromContract(Contract contract);

        DateTime? GetNextPaymentDateByCreditLineId(int creditLineId);

        bool IsDefaultScheduleType(PercentPaymentType percentPaymentType, ScheduleType? scheduleType);
        List<RestructuredContractPaymentSchedule> BuildAfterDefermentSchedulePeriod(int restructuredMonthCount, List<ContractPaymentSchedule> schedulePartAfterDeferment,
            decimal amortizedBalanceOfDefferedPercent = 0, decimal amortizedBalanceOfOverduePercent = 0, decimal amortizedPenaltyOfOverdueDebt = 0, decimal amortizedPenaltyOfOverduePercent = 0);
    }
}
