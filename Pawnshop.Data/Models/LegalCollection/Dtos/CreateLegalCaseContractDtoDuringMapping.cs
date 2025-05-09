namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class CreateLegalCaseContractDtoDuringMapping
    {
        public int ContractId { get; set; }
        public int InscriptionId { get; set; }
        public string ContractNumber { get; set; }
        public string CollectionStatusCode { get; set; }
    }
}