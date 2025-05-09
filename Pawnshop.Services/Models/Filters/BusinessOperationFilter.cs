
namespace Pawnshop.Services.Models.Filters
{
    public class BusinessOperationFilter
    {
        public int? TypeId { get; set; }
        public string Code { get; set; }
        public int? OrganizationId { get; set; }
        public int? BranchId { get; set; }
        public int? AccountId { get; set; }
        public bool? IsManual { get; set; }
    }
}
