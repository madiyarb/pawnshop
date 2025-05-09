using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Restructuring;

namespace Pawnshop.Services.Models.Filters
{
    public class ContractFilter
    {
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CollateralType? CollateralType { get; set; }
        public ContractDisplayStatus? DisplayStatus { get; set; }
        public int? ClientId { get; set; }
        public int[] OwnerIds { get; set; }
        public bool? IsTransferred { get; set; }
        public string IdentityNumber { get; set; }
        public ContractStatus? Status { get; set; }
        public int? OrganizationId { get; set; }
        public bool? HasParent { get; set; }
        public bool? HasFcbChecked { get; set; }
        public List<ContractStatus> Statuses { get; set; }
        public int? PositionId { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        /// <summary>
        /// Дата оплаты(не следующая/просроченная)
        /// </summary>
        public DateTime? PaymentDate { get; set; }
        public DateTime? NextPaymentEndDate { get; set; }
        public bool? IsNotInscription { get; set; }
        public string ContractNumber { get; set; }
    }
}
