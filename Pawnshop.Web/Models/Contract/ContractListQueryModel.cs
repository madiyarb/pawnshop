using System;
using System.Collections.Generic;
using Org.BouncyCastle.Math;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Web.Models.Contract
{
    public class ContractListQueryModel
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public CollateralType? CollateralType { get; set; }

        public ContractDisplayStatus? DisplayStatus { get; set; }

        public int? ClientId { get; set; }

        public int[] OwnerIds { get; set; }

        public bool IsTransferred { get; set; }

        public string IdentityNumber { get; set; }

        public ContractStatus? Status { get; set; }
        
        public int? OrganizationId { get; set; }

        public bool? HasParent { get; set; }

        public bool? HasFcbChecked { get; set; }

        public List<ContractStatus> Statuses { get; set; }
        public bool AllBranches { get; set; }

        public string? CarNumber { get; set; }
        public string? Rca { get; set; }
        
        public int? SettingId { get; set; }
    }
}