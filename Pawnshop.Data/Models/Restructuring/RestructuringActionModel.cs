using Pawnshop.Data.Models.ClientDeferments;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Restructuring
{
    public class RestructuringActionModel
    {
        public ContractActionType RestructurionActionType { get; set; }
        public Contract Contract { get; set; }
        public Group Branch { get; set; }
        public ClientDeferment ClientDeferment { get; set; }
        public DateTime StartDefermentDate { get; set; }
        public DateTime EndDefermentDate { get; set; }
        public int DefermentTypeId { get; set; }
        public int AuthorId { get; set; }
    }
}
