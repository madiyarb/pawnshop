using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.Contracts.Kdn
{
    public class FCBReport : IEntity
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string FolderName { get; set; }
        public string PdfFileLink { get; set; }
        public string XmlFileLink { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        [JsonIgnore]
        public User Author { get; set; }
        public DateTime DeleteDate { get; set; }
        public FCBReportTypeCode ReportType { get; set; }
    }
}
