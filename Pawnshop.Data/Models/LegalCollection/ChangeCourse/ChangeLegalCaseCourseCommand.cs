using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Data.Models.LegalCollection.ChangeCourse
{
    public class ChangeLegalCaseCourseCommand
    {
        public int LegalCaseId { get; set; }
        public int ClientId { get; set; }
        public ChangeCourseActionDto ChangeCourseAction { get; set; }
        public string? Note { get; set; }
        public int? AuthorId { get; set; }
    }
}