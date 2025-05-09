namespace Pawnshop.Data.Models.LegalCollection
{
    public class Model
    {
        public string? ContractNumber { get; set; }
        public string? IdentityNumber { get; set; }
        public string? FullName { get; set; }
        public string? CarNumber { get; set; }
        public string? RKA { get; set; }
        public int? BranchId { get; set; }
        public int? ParkingStatusId { get; set; }
        public int? CollateralType { get; set; }
        public int? RegionId { get; set; }
        
        public int? StatusId { get; set; }
        public int? CourseId { get; set; }
        public int? StageId { get; set; }
        public int? TaskStatusId { get; set; }
        public bool? HasDebtProcessByContractIin { get; set; }
    }
}