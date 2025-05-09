using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ContractDataForAddition
    {
        public decimal LoanCost { get; set; }
        public int DelayDayCount { get; set; }
        public string ContractNumber { get; set; }
        public string Car { get; set; }
        public decimal BuyoutCost { get; set; }
        public decimal DebtLeftCost { get; set; }
        public decimal PercentCost { get; set; }
        public decimal PenyCost { get; set; }
        public decimal EstimatedCost { get; set; }
        private int? parentContractId = null;
        public int? ParentContractId
        {
            get { return parentContractId; }
            set { parentContractId = value == 0 || value is null ? null : value; }
        }
    }
}
