using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Clients;
using System;

namespace Pawnshop.Services.Models.UKassa
{
    public class CashOrderUKassaReportDto
    {
        public int OrderId { get; set; }
        public int RequestId { get; set; }
        public OrderType OrderType { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderCost { get; set; }
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }
        public string AuthorName { get; set; }
        public string Reason { get; set; }
        public string CheckNumber { get; set; }

        public CashOrderUKassaStatusEnum Status { get; set; }
    }
}
