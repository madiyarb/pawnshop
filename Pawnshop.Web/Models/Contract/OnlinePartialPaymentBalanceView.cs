using System.Net;

namespace Pawnshop.Web.Models.Contract
{
    public class OnlinePartialPaymentBalanceView : BaseResponse
    {
        /// <summary>
        /// Сумма на авансе
        /// </summary>
        public decimal Depo { get; set; }

        /// <summary>
        /// Сумма ОД после ЧДП
        /// </summary>
        public decimal MainDebtAfterCDP { get; set; }

        /// <summary>
        /// Сумма процентов
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Сумма ОД
        /// </summary>
        public decimal MainDebt { get; set; }

        /// <summary>
        /// Сумма пополнения аванса
        /// </summary>
        public decimal NeedToPrepayment { get; set; }


        public OnlinePartialPaymentBalanceView(HttpStatusCode statusCode, string message) : base(statusCode, message) { }

        public OnlinePartialPaymentBalanceView(HttpStatusCode statusCode, string message, decimal depo, decimal mainDebtAfterCDP, decimal percent, decimal mainDebt, decimal needToPrepayment) : base(statusCode, message)
        {
            Depo = depo;
            MainDebtAfterCDP = mainDebtAfterCDP;
            Percent = percent;
            MainDebt = mainDebt;
            NeedToPrepayment = needToPrepayment;
        }
    }
}
