namespace Pawnshop.Web.Models.AbsOnline
{
    public class ApplicationReceiverAccountRequest
    {
        /// <summary>
        /// Параметр шины <b><u>bank</u></b>
        /// </summary>
        public string Bank { get; set; }

        /// <summary>
        /// Параметр шины <b><u>card_date</u></b>
        /// </summary>
        public string CardDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>cardholder_name</u></b>
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// Параметр шины <b><u>card_number</u></b>
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>card_url</u></b>
        /// </summary>
        public string CardUrl { get; set; }

        /// <summary>
        /// Параметр шины <b><u>client_name</u></b>
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>cvc</u></b>
        /// </summary>
        public string CVC { get; set; }

        /// <summary>
        /// Параметр шины <b><u>email</u></b>
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Параметр шины <b><u>iban</u></b>
        /// </summary>
        public string Iban { get; set; }

        /// <summary>
        /// Параметр шины <b><u>uin</u></b>
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tel</u></b>
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Параметр шины <b><u>payment_type</u></b>
        /// </summary>
        public int PaymentType { get; set; }
    }
}
