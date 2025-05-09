namespace Pawnshop.Data.Models.DebtorRegistry.CourtOfficer
{
    /// <summary>
    /// ДТО для "Частного судебного исполнителя"
    /// </summary>
    public class CourtOfficerDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? DepartmentName { get; set; }
        public string? Region { get; set; }
        public string? PhoneNumber { get; set; }
    }
}