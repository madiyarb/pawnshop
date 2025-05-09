namespace Pawnshop.Data.Models.LegalCollection.Documents
{
    public class UploadLegalCaseDocumentCommand
    {
        public int? LegalCaseActionId { get; set; }
        public int LegalCaseId { get; set; }
        public int FileId { get; set; }
        public int? DocumentTypeId { get; set; }
    }
}
