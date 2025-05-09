using System;

namespace Pawnshop.Web.Models.FCBReport
{
    public class FCBReportView
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string FolderName { get; set; }
        public string PdfFileLink { get; set; }
        public string XmlFileLink { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public DateTime DeleteDate { get; set; }
    }
}
