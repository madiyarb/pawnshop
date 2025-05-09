using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class RealtyDocument : IEntity
    {
        public int Id { get; set; }
        public int RealtyId { get; set; }
        public int DocumentTypeId { get; set; }
        public string? Number { get; set; }
        public DateTime? Date { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? IssuedBy { get; set; }
        public ClientDocumentType? DocumentType { get; set; }
    }
}
