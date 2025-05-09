namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class ClientDto
    {
        public int? Id { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }
        public string? FullName { get; set; }
    }
}