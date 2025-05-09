
namespace Pawnshop.Data.Models.LegalCollection.Documents
{
    public class CreateLegalCaseDocumentCommand
    {
        public int? LegalCaseActionId { get; set; }
        public string FileName { get; set; }
        public int FileId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorFullName { get; set; }
        public int LegalCaseId { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }
        public int? DocumentTypeId { get; set; }
    }
}
