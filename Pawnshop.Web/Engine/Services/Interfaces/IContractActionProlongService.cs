using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IContractActionProlongService
    {
        void Exec(ContractAction action, int authorId, int branchId, bool forceExpensePrepaymentReturn, bool autoApprove = false);
        void ExecOnApprove(ContractAction action, int authorId, Contract contract = null);
    }
}
