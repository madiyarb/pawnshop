using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Online1C
{
    /// <summary>Кассовые операции, не относящиеся к кредитам</summary>
    public class Online1CReportCashFlows : IEntity
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string DT1C { get; set; }
        public string DTSubkonto { get; set; }
        public string OneCCodeDT { get; set; }
        public string KT1C { get; set; }
        public string KTSubkonto { get; set; }
        public string OneCCodeKT { get; set; }
        public decimal OrderCost { get; set; }
        public string OrderBranch { get; set; }
        public string Client { get; set; }
        public string ClientIIN { get; set; }
    }
}