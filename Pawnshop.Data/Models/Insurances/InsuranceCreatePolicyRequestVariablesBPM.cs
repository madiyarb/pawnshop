using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Models.Insurances
{
    public class InsuranceCreatePolicyRequestVariablesBPM
    {
        [JsonProperty("V_REQUEST_ID")]
        public string RequestId { get; set; }

        [JsonProperty("V_CONTRACT_ID")]
        public string ContractId { get; set; }

        [JsonProperty("V_AUTHOR_ID")]
        public string AuthorId { get; set; }

        [JsonProperty("V_CLIENT_ID")]
        public string ClientId { get; set; }

        [JsonProperty("V_POLICY_NUMBER")]
        public string PolicyNumber { get; set; }

        [JsonProperty("V_CLIENT_ADDRESS")]
        public string ClientAddress { get; set; }

        [JsonProperty("V_CLIENT_FULLNAME")]
        public string ClientFullname { get; set; }

        [JsonProperty("V_CLIENT_IIN")]
        public string ClientIin { get; set; }

        [JsonProperty("V_CLIENT_BIRTH_DATE")]
        public string ClientBirthDate { get; set; }

        [JsonProperty("V_CLIENT_EMAIL")]
        public string ClientEmail { get; set; }

        [JsonProperty("V_CLIENT_MOBILE_PHONE")]
        public string ClientMobilePhone { get; set; }

        [JsonProperty("V_CLIENT_RESIDENT")]
        public string ClientResident { get; set; }

        [JsonProperty("V_CREDIT_SUM")]
        public string CreditSum { get; set; }

        [JsonProperty("V_CLIENT_DOC_DATE")]
        public string ClientDocDate { get; set; }

        [JsonProperty("V_CLIENT_DOC_NUMBER")]
        public string ClientDocNumber { get; set; }

        [JsonProperty("V_CLIENT_DOC_TYPE")]
        public string ClientDocType { get; set; }

        [JsonProperty("V_CLIENT_DOC_ISSUED_BY")]
        public string ClientDocIssuedBy { get; set; }

        [JsonProperty("V_POLICY_START")]
        public string PolicyStart { get; set; }

        [JsonProperty("V_POLICY_STOP")]
        public string PolicyStop { get; set; }

        [JsonProperty("V_POLICY_PERCENT")]
        public string PolicyPercent { get; set; }

        [JsonProperty("V_POLICY_TERM_LOAN")]
        public string PolicyTermLoan { get; set; }

        [JsonProperty("V_CLIENT_LAST_NAME")]
        public string ClientLastName { get; set; }

        [JsonProperty("V_CLIENT_FIRST_NAME")]
        public string ClientFirstName { get; set; }

        [JsonProperty("V_CLIENT_MIDDLE_NAME")]
        public string ClientMiddleName { get; set; }

        [JsonProperty("V_CLIENT_BIRTH_PLACE")]
        public string ClientBirthPlace { get; set; }

        [JsonProperty("V_CLIENT_CHANGED_SURNAME")]
        public string ClientChangedSurname { get; set; }

        [JsonProperty("V_CLIENT_IS_GBDFL")]
        public string ClientIsGbdfL { get; set; }

        [JsonProperty("V_CLIENT_DATE_GBDFL")]
        public string ClientDateGbdfL { get; set; }

        [JsonProperty("V_CLIENT_DOC_SERIES")]
        public string ClientDocSeries { get; set; }

        [JsonProperty("V_CLIENT_DOC_TYPE_ID")]
        public string ClientDocTypeCode { get; set; }

        [JsonProperty("V_CLIENT_FAMILY_STATUS")]
        public string ClientFamilyStatus { get; set; }

        [JsonProperty("V_CLIENT_HOME_PHONE")]
        public string ClientHomePhone { get; set; }

        [JsonProperty("V_CLIENT_JOB")]
        public string ClientJob { get; set; }

        [JsonProperty("V_CLIENT_MOBILE_PHONE2")]
        public string ClientMobilePhone2 { get; set; }

        [JsonProperty("V_CLIENT_NOTE")]
        public string ClientNote { get; set; }

        [JsonProperty("V_CLIENT_POSITION")]
        public string ClientPosition { get; set; }

        [JsonProperty("V_CLIENT_REGION_KATOCODE")]
        public string ClientRegionKatocode { get; set; }

        [JsonProperty("V_CLIENT_RESIDENT_CODE")]
        public string ClientResidentCode { get; set; }

        [JsonProperty("V_CLIENT_SEX")]
        public string ClientSex { get; set; }

        [JsonProperty("V_CLIENT_WORK_PHONE")]
        public string ClientWorkPhone { get; set; }

        [JsonProperty("V_POLICY_BANK_AV")]
        public string PolicyBankAv { get; set; }

        [JsonProperty("V_POLICY_BORDERO_NUMBER")]
        public string PolicyBorderoNumber { get; set; }

        [JsonProperty("V_POLICY_INSURANCE_PREMIUM")]
        public string PolicyInsurancePremium { get; set; }

        [JsonProperty("V_POLICY_INSURANCE_SUM")]
        public string PolicyInsuranceSum { get; set; }

        [JsonProperty("V_POLICY_IS_ONLINE")]
        public string PolicyIsOnline { get; set; }

        [JsonProperty("V_POLICY_CONCLUSION_DATE")]
        public string PolicyConclusionDate { get; set; }

        [JsonProperty("V_POLICY_DATE_LOAN")]
        public string PolicyDateLoan { get; set; }

        [JsonProperty("V_POLICY_ID")]
        public string PolicyId { get; set; }

        [JsonProperty("V_POLICY_ISSUE_CITY")]
        public string PolicyIssueCity { get; set; }

        [JsonProperty("V_POLICY_ISSUE_DATE")]
        public string PolicyIssueDate { get; set; }

        [JsonProperty("V_POLICY_DATE_REQUEST")]
        public string PolicyDateRequest { get; set; }



        public InsuranceCreatePolicyRequestVariablesBPM(Guid requestGuid, InsurancePoliceRequest policeRequest, Client client, Contract contract, int userId, string maritalStatusName, string highestAteKatoCode, string branchName)
        {
            var today = DateTime.Now.Date;
            var requestData = policeRequest.RequestData;
            var clientDocument = client.Documents.FirstOrDefault(x => !x.DocumentType.Disabled &&
                                        x.DocumentType.IsIndividual
                                        && new List<string>() { "PASSPORTKZ", "IDENTITYCARD", "PASSPORTRU", "FOREIGN_PASSPORT" }.Contains(x.DocumentType.Code));

            RequestId = requestGuid.ToString();
            ContractId = policeRequest.ContractId.ToString();
            AuthorId = userId.ToString();
            ClientId = client.Id.ToString();
            PolicyNumber = $"TFG-{today.Day.ToString().PadLeft(2, '0')}{today.Month.ToString().PadLeft(2, '0')}{today.Year}-{policeRequest.ContractId}";
            ClientAddress = requestData.Address;
            ClientFullname = requestData.FullName;
            ClientIin = requestData.IdentityNumber;
            ClientBirthDate = requestData.BirthDay.ToString("dd.MM.yyyy");
            ClientEmail = requestData.Email;
            ClientMobilePhone = requestData.PhoneNumber;
            ClientResident = requestData.IsResident.ToString();
            CreditSum = contract.LoanCost.ToString();
            ClientDocDate = requestData.DocumentDate.ToString("dd.MM.yyyy");
            ClientDocNumber = requestData.DocumentNumber;
            ClientDocType = requestData.DocumentType.ToString();
            ClientDocIssuedBy = requestData.Provider;
            PolicyStart = DateTime.Now.Date.ToString("dd.MM.yyyy");
            PolicyStop = DateTime.Now.Date.AddYears(1).AddDays(-1).ToString("dd.MM.yyyy");
            PolicyTermLoan = Constants.OneYearInsurance;
            ClientLastName = client.Surname;
            ClientFirstName = client.Name;
            ClientMiddleName = client.MaidenName is null ? "" : client.MaidenName;
            ClientBirthPlace = Constants.KZTCountryName;
            ClientChangedSurname = client.MaidenName is null ? "" : client.MaidenName;
            ClientIsGbdfL = "0";
            ClientDateGbdfL = "";
            ClientDocSeries = clientDocument.Series is null ? "" : clientDocument.Series;
            ClientDocTypeCode = clientDocument.DocumentType.Code;
            ClientFamilyStatus = maritalStatusName;
            ClientHomePhone = "";
            ClientWorkPhone = "";
            ClientMobilePhone2 = "";
            ClientJob = "";
            ClientNote = "";
            ClientPosition = "";
            ClientRegionKatocode = highestAteKatoCode;
            ClientResidentCode = client.Citizenship.NameRus;
            ClientSex = client.IsMale.Value ? Constants.MaleName : Constants.FemaleName;
            PolicyBankAv = "";
            PolicyBorderoNumber = "";
            PolicyInsurancePremium = requestData.InsurancePremium.ToString();
            PolicyInsuranceSum = requestData.InsuranceAmount.ToString();
            PolicyIsOnline = "0";
            PolicyConclusionDate = contract.ContractDate.ToString("dd.MM.yyyy");
            PolicyDateLoan = contract.ContractDate.ToString("dd.MM.yyyy");
            PolicyId = "";
            PolicyIssueCity = branchName;
            PolicyIssueDate = contract.ContractDate.ToString("dd.MM.yyyy");
            PolicyDateRequest = contract.ContractDate.ToString("dd.MM.yyyy");
            PolicyPercent = requestData.InsuranceRate.ToString();
        }
    }
}
