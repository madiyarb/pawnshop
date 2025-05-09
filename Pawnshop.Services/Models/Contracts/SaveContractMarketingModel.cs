namespace Pawnshop.Services.Models.Contracts
{
    public class SaveContractMarketingModel
    {
        public int ContractId { get; set; }
        public int AttractionChannelId { get; set; }
        public int LoanPurposeId { get; set; }
        public int? BusinessLoanPurposeId { get; set; }
        public int? OkedForIndividualsPurposeId { get; set; }
        public int? TargetPurposeId { get; set; }
        public string OtherLoanPurpose { get; set; }
    }
}