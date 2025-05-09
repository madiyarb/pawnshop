using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.PayOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.PayOperation
{
    public class PayOperationListQueryModel
    {
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BranchId { get; set; }
        public List<int> BranchIds { get; set; } = new List<int>();
        public int? OrganizationId { get; set; }
        public List<int> OrganizationIds { get; set; } = new List<int>();
        public int? ClientId { get; set; }
        public int? PayTypeId { get; set; }
        public int? RequisiteTypeId { get; set; }
        public int? RequisiteId { get; set; }
        public ContractActionType? ActionType { get; set; }
        public PayOperationStatus? Status { get; set; }
    }
}
