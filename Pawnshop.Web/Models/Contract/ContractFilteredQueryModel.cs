using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using System;

namespace Pawnshop.Web.Models.Contract
{
    public class ContractFilteredQueryModel
    {
        public string IdentityNumber { get; set; }

        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public CollateralType? CollateralType { get; set; }

        public ContractDisplayStatus? DisplayStatus { get; set; }

        public string? CarNumber { get; set; }

        public string? Rca { get; set; }

        public bool IsTransferred { get; set; }

        public int? ClientId { get; set; }

        public int[] OwnerIds { get; set; }

        public int? SettingId { get; set; }

        public bool AllBranches { get; set; }

        public bool? CreatedInOnline { get; set; }
    }
}
