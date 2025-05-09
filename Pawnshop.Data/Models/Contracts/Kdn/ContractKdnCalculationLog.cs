using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Kdn
{
    public class ContractKdnCalculationLog : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public int ContractId { get; set; }
        public decimal KDNCalculated { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }
        public bool IsKdnPassed { get; set; }
        public string KdnError { get; set; }
        public decimal TotalAspIncome { get; set; }
        public decimal TotalContractDebts { get; set; }
        public decimal TotalOtherClientPayments { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal TotalFcbDebt { get; set; }
        public decimal TotalFamilyDebt { get; set; }
        public bool IsAddition { get; set; }
        public int? ParentContractId { get; set; }
        public DateTime UpdateDate { get; set; }
        public int? ChildSettingId { get; set; }
        public int? ChildLoanPeriod { get; set; }
        public int? ChildSubjectId { get; set; }
        public int? PositionEstimatedCost { get; set; }
        public bool IsGambler { get; set; }
        public bool IsStopCredit { get; set; }
        public decimal KDNK4Calculated {  get; set; }
    }
}
