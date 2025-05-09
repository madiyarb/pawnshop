using System;

namespace Pawnshop.Services.ApplicationsOnline.ApplicationOnlineCreditLimitVerification
{
    public sealed class ApplicationOnlineControlSums
    {
        public decimal RequestedAmount { get; set; }
        public decimal CurrentMainDebtAmount { get; set; }
        public decimal CarCreditLineLimitAmount { get; set; }
        public decimal CreditLineLimitAmount { get; set;}

        public bool Validate()
        {
            decimal maxLimit = 0;

            if (CreditLineLimitAmount != 0 && CarCreditLineLimitAmount != 0)
            {
                maxLimit = CreditLineLimitAmount;
                if (CreditLineLimitAmount > CarCreditLineLimitAmount)
                {
                    maxLimit = CarCreditLineLimitAmount;
                }
            }
            else
            {
                maxLimit = CarCreditLineLimitAmount;
            }

            if (maxLimit < RequestedAmount + CurrentMainDebtAmount)
            {
                return false;
            }

            return true;
        }

        public string Comment()
        {
            if (Validate())
                return String.Empty;
            return $"Общий основной долг с учетом текущего долга = {CurrentMainDebtAmount} и запрашиваемой суммы = {RequestedAmount} . " +
                   $"Общая сумма =  {CurrentMainDebtAmount + RequestedAmount} ." +
                   $"Меньше максимального лимита по оценке автомобиля = {CarCreditLineLimitAmount} . Либо текущего лимита кредитной линии {CreditLineLimitAmount} .";
        }
    }
}
