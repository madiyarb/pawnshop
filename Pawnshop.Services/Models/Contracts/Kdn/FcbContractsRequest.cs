using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class FcbContractsRequest
    {
        public FcbReportRequest FcbReportRequest { get; set; }
        public int ContractId { get; set; }
        public int SubjectTypeId { get; set; }
        public bool IsFromAdditionRequest { get; set; }
    }
}
