using Microsoft.AspNetCore.Http;
using System;

namespace Pawnshop.Data.Models.Restructuring
{
    public class RestructuringSaveModel
    {
        public int ContractId { get; set; }
        public int DefermentTypeId { get; set; }
        public DateTime StartDefermentDate { get; set; }
        public DateTime EndDefermentDate { get; set; }
        public int RestructuredMonthCount { get; set; }
        public string RestructuredSchedule { get; set; }
        public IFormFile DocumentFile { get; set; }
        public string Apr { get; set; }
    }
}
