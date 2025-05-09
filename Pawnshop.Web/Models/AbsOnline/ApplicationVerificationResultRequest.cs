using System;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class ApplicationVerificationResultRequest
    {

        /// <summary>
        /// Параметр шины <b><u>tel2</u></b>
        /// </summary>
        public string AdditionalPhone { get; set; }

        /// <summary>
        /// Параметр шины <b><u>approved_stage</u></b>
        /// </summary>
        public int ApprovedStage { get; set; }

        /// <summary>
        /// Параметр шины <b><u>bank</u></b>
        /// </summary>
        public string Bank { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_color</u></b>
        /// </summary>
        public string CarColor { get; set; }

        /// <summary>
        /// Параметр шины <b><u>cardholder_name</u></b>
        /// </summary>
        public string CardholderName { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_vin</u></b>
        /// </summary>
        public string CarVin { get; set; }

        /// <summary>
        /// Параметр шины <b><u>client_name</u></b>
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Параметр шины <b><u>contract_address</u></b>
        /// </summary>
        public string ContractAddress { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>email</u></b>
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Параметр шины <b><u>gender</u></b>
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Параметр шины <b><u>uin</u></b>
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_insurance</u></b>
        /// </summary>
        public bool? Insurance { get; set; }

        /// <summary>
        /// Параметр шины <b><u>kato_address</u></b>
        /// </summary>
        public string KatoAddress { get; set; }

        /// <summary>
        /// Параметр шины <b><u>LTV</u></b>
        /// </summary>
        public decimal LTV { get; set; }

        /// <summary>
        /// Параметр шины <b><u>market_cost</u></b>
        /// </summary>
        public int? MarketCost { get; set; }

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
        /// Параметр шины <b><u>passport_title</u></b>
        /// </summary>
        public string PassportTitle { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tech_passport_doc_at</u></b>
        /// </summary>
        public DateTime? TechPassportIssueDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tech_passport_issuing_org</u></b>
        /// </summary>
        public string TechPassportIssuer { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tech_passport_docnum</u></b>
        /// </summary>
        public string TechPassportNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tech_passport_title</u></b>
        /// </summary>
        public string TechPassportTitle { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_value</u></b>
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>verification_result</u></b>
        /// </summary>
        public string VerificationResult { get; set; }

        /// <summary>
        /// Список рефинансируемых займов 
        /// </summary>
        public List<string> RefinanceList { get; set; } = new List<string>();

    }
}
