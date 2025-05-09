namespace Pawnshop.Web.Models.AbsOnline
{
    public class ApplicationPaymentDayRequest
    {
        /// <summary>
        /// Параметр шины <b><u>contract_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>crm_tas_loan_req</u></b>
        /// </summary>
        public int CrmLoanReq { get; set; }

        /// <summary>
        /// Параметр шины <b><u>error</u></b>
        /// </summary>
        public int Error { get; set; }

        /// <summary>
        /// Параметр шины <b><u>uin</u></b>
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>message</u></b>
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Параметр шины <b><u>new_day_pay</u></b>
        /// </summary>
        public int PayDay { get; set; }
    }
}
