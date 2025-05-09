using Pawnshop.Data.Models.Collection;

namespace Pawnshop.Data.Models.LegalCollection.Create
{
    public class CreateLegalCaseCommand
    {
        public int ContractId { get; set; }
        public string? ContractNumber { get; set; }
        public int? DelayCurrentDay { get; set; }
        public int? CaseReasonId { get; set; }
        public string BranchName { get; set; }
        public string ClientFullName { get; set; }
        public string ClientIIN { get; set; }
        public CollectionReason? Reason { get; set; }
    }
}