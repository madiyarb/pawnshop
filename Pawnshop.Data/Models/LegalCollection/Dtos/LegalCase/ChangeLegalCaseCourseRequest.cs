namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class ChangeLegalCaseCourseRequest
    {
        public int LegalCaseId { get; set; }
        public string ActionCode { get; set; }
        public string? Note { get; set; }
        public int? AuthorId { get; set; }
    }
}