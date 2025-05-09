namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class ContractInfoCollectionStatusDto
    {
        public int ContractId { get; set; }
        public int FincoreStatusId { get; set; }
        public string CollectionStatusCode { get; set; }
        public bool IsActive { get; set; }
        public int? DelayDays { get; set; }
        public string ClientIdentityNumber { get; set; }
        public string ClientFullName { get; set; }
        public string GroupDisplayName { get; set; }
    }
}