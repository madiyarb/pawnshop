namespace Pawnshop.Data.Models.DebtorRegistry
{
    public class DebtRegistryModel
    {
        public int? CourtOfficerId { get; set; }
        public string? CourtOfficerRegion { get; set; }
        public bool? IsTravelBan { get; set; }
        public string? IdentityNumber { get; set; }
        public string? ClientFullName { get; set; }
    }
}