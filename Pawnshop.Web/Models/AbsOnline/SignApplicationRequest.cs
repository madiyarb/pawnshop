namespace Pawnshop.Web.Models.AbsOnline
{
    public class SignApplicationRequest
    {
        /// <summary>
        /// Параметр шины <b><u>contract_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>uin</u></b>
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tel</u></b>
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Параметр шины <b><u>sms</u></b>
        /// </summary>
        public string SmsCode { get; set; }
    }
}
