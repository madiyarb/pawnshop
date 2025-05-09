using System;

namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    /// <summary>
    /// Документ дела Legal case 
    /// </summary>
    public class LegalCaseDocumentViewModel
    {
        public int Id { get; set; }
        public int LegalCaseId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorFullName { get; set; }
        public int? ActionId { get; set; }
        public string ActionName { get; set; }
        public string FileName { get; set; }
        public int FileId { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public LegalCollectionDocumentTypeDto? DocumentType { get; set; }
    }
}