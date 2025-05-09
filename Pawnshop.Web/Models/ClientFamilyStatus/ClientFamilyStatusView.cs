namespace Pawnshop.Web.Models.ClientFamilyStatus
{
    public sealed class ClientFamilyStatusView
    {
        public int? MaritalStatusId { get; set; }
        public string? SpouseFullname { get; set; }
        public int? SpouseIncome { get; set; }
        public int? ChildrenCount { get; set; }
        public int? AdultDependentsCount { get; set; }
        public int? UnderageDependentsCount { get; set; }
    }
}
