using Pawnshop.Data.Models.Contracts;
using Pawnshop.Web.Models.AbsOnline;

namespace Pawnshop.Web.Converters
{
    public static class ContractPaymentScheduleConverter
    {
        public static PaymentScheduleViewModel ToOnlineViewModel(this ContractPaymentSchedule schedule, int? number)
        {
            return new PaymentScheduleViewModel
            {
                Date = schedule.Date,
                Number = number,
                Amount = schedule.DebtCost + schedule.PercentCost,
                PrincipalDebtLeft = schedule.DebtLeft,
                PrincipalDebt = schedule.DebtCost,
                ProfitAmount = schedule.PercentCost,
            };
        }
    }
}
