using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Models.Contracts.Kdn;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IContractActionAdditionService
    {
        void Exec(ContractAction action,int authorId, int branchId, bool unsecuredContractSignNotallowed, bool forceExpensePrepaymentReturn);
    }
}
