using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.LegalCollection.Documents
{
    public class LegalCaseDocumentResponse
    {
        public int Id { get; set; }
        public DateTimeOffset UploadDate { get; set; }
        public int? LegalCaseActionId { get; set; }
        public string FileName { get; set; }
        public int FileId { get; set; }
        public int AuthorId { get; set; }
        public int LegalCaseId { get; set; }
        public DateTimeOffset? DeleteDate { get; set; }
    }
}
