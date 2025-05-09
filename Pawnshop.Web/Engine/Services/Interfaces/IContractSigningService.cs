using System;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IContractSigningService
    {
        /// <summary>
        /// Sign tranche and linked credit line 
        /// </summary>
        /// <param name="contractId">Contract identifier</param>
        /// <param name="authorId">User identifier</param>
        /// <param name="requisiteId">Requisite identifier</param>
        /// <exception cref="ClientRequisiteNotFoundException">Client haven't requisite to cashout</exception>
        /// <exception cref="ContractClassWrongException">Contract is not tranche</exception>
        /// <exception cref="ContractNotFoundException">Contract not found</exception>
        /// <exception cref="CreditLineNotFoundException">Credit line not found</exception>
        /// <exception cref="ContractInWrongStatus">Contract not in draft status can't sign</exception>
        public Task SignTrancheAndCreditLine(int contractId, int authorId, int branchId, int? requisiteId = null, bool onlyOnline = true, int? cashIssueBranchId = null);

        public void SignTranche(Contract contract, int authorId, ClientRequisite clientRequisite, PayType payType, int? cashIssueBranchId = null);

        public void SignCreditLine(Contract contract, int authorId, ClientRequisite clientRequisite, PayType payType);
    }
}
