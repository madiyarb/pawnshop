namespace Pawnshop.Data.Models.LegalCollection.DocumentType.HttpServie
{
    public class UpdateDocumentTypeHttpRequest
    {
        public int DocumentTypeId { get; set; }
        public string? Name { get; set; }
        public string? AlternativeName { get; set; }
        public string? Code { get; set; }
    }
}