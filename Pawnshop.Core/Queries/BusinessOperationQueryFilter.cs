namespace Pawnshop.Core.Queries
{
    public class BusinessOperationQueryFilter
    {
        public string Code { get; set; }
        public int? TypeId { get; set; }
        public int? OrganizationId { get; set; }
        public int? BranchId { get; set; }
        public int? AccountId { get; set; }
        public bool? IsManual { get; set; }
    }
}