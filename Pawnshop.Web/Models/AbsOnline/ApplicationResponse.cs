using System.Collections.Generic;
using System;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class ApplicationResponse
    {
        /// <summary>
        /// Параметр шины <b><u>tel2</u></b>
        /// </summary>
        public string AdditionalPhone { get; set; }

        /// <summary>
        /// Параметр шины <b><u>address</u></b> или <b><u>contract_address</u></b> (непонятно)
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Параметр шины <b><u>bank</u></b>
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Параметр шины <b><u>birth_date</u></b>
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_brand</u></b>
        /// </summary>
        public string Car { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_color</u></b>
        /// </summary>
        public string CarColor { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_cost</u></b>
        /// </summary>
        public decimal CarCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_id</u></b>
        /// </summary>
        public string CarId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_vin</u></b>
        /// </summary>
        public string CarVin { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_year</u></b>
        /// </summary>
        public int? CarYear { get; set; }

        /// <summary>
        /// Параметр шины <b><u>address</u></b> или <b><u>contract_address</u></b> (непонятно)
        /// </summary>
        public string ClientAddress { get; set; }

        /// <summary>
        /// Параметр шины <b><u>client_name</u></b>
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>email</u></b>
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Параметр шины <b><u>firstname</u></b>
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Параметр шины <b><u>iban</u></b>
        /// </summary>
        public string Iban { get; set; }

        /// <summary>
        /// Параметр шины <b><u>uin</u></b>
        /// </summary>  
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>lastname</u></b>
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_value</u></b>
        /// </summary>
        public decimal? LoanCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>market_cost</u></b>
        /// </summary>
        public decimal? MarketCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>middlename</u></b>
        /// </summary>
        public string Middlename { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tel</u></b>
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Параметр шины <b><u>partner_id</u></b>
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Параметр шины <b><u>passport_doc_at</u></b>
        /// </summary>
        public DateTime? PassportIssueDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>passport_issuing_org</u></b>
        /// </summary>
        public string PassportIssuer { get; set; }

        /// <summary>
        /// Параметр шины <b><u>passport_docnum</u></b>
        /// </summary>
        public string PassportNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_id</u></b>
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>refinance_list</u></b>
        /// </summary>
        public List<string> RefinanceList { get; set; } = new List<string>();

        /// <summary>
        /// Параметр шины <b><u>tech_passport_doc_at</u></b>
        /// </summary>
        public DateTime? TechPassportIssueDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tech_passport_docnum</u></b>
        /// </summary>
        public string TechPassportNumber { get; set; }

    }
}
