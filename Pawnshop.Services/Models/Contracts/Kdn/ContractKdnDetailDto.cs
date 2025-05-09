using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class ContractKdnDetailDto
    {
        /// <summary>
        /// Идентификатор детализации
        /// </summary>
        public int Id { get; set; }

        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public int SubjectTypeId { get; set; }
        public decimal MonthlyPaymentAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public int? FileRowId { get; set; }
        public FileRow FileRow { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }
        public string CreditorName { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public decimal ContractTotalAmount { get; set; }
        public int ForthcomingPaymentCount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int OverdueDaysCount { get; set; }
        public string CollateralType { get; set; }
        public decimal CollateralCost { get; set; }
        public bool IsLoanPaid { get; set; }
        public bool IsCreditCard { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? UserUpdated { get; set; }
        public bool IsFromAdditionRequest { get; set; }
        public DateTime? AdditionRequestDate { get; set; }
    }
}
