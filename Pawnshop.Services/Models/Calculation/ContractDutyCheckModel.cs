using Pawnshop.Data.Models.Contracts.Actions;
using System;

namespace Pawnshop.Services.Models.Calculation
{
    public class ContractDutyCheckModel
    {
        public int ContractId { get; set; }
        public int? PayTypeId { get; set; }
        public ContractActionType ActionType { get; set; }
        public DateTime Date { get; set; } = DateTime.Now.Date;
        public decimal Cost { get; set; }
        public int? EmployeeId { get; set; }
        public bool? IsReceivable { get; set; }
        public decimal? Refinance { get; set; } = 0;
        public int? BranchId { get; set; }
    }
}
