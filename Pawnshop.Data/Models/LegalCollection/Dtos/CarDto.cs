namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class CarDto
    {
        public int? Id { get; set; }
        public string? TransportNumber { get; set; }
        public string? Mark { get; set; }
        public string? Model { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? BodyNumber { get; set; }
        public string? Color { get; set; }
        public int? ReleaseYear { get; set; }
    }
}