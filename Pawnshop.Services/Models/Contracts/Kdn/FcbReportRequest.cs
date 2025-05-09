using Pawnshop.Data.Models.Contracts.Kdn;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class FcbReportRequest
    {
        public int OrganizationId { get; set; }
        public string Author { get; set; }
        public int? DocumentType { get; set; }
        public string IIN { get; set; }
        public int Creditinfoid { get; set; }
        public int ClientId { get; set; }
        public int AuthorId { get; set; }
        public FCBReportTypeCode? ReportType { get; set; }
    }
}
