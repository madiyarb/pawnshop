using System.Collections.Generic;

namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class RegionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? NameAlt { get; set; }
        public List<GroupDto>? Groups { get; set; }
    }
}