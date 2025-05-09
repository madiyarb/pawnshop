using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
namespace Pawnshop.Data.Models.InteresAccrual
{
    public class InterestAccrualModel
    {
        public Dictionary<AmountType, decimal> AccrualDict {  get; set; }
        public DateTime OperationDate { get; set; }
        public string RequestLogJson { get; set; }
    }
}
