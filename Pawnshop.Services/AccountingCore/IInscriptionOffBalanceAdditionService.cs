using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.AccountingCore
{
    public interface IInscriptionOffBalanceAdditionService
    {
        Task AddOffbalancePaymentForContractsWithInscription(int branchId, DateTime? inputEndDate = null);
        Task AddOffBalanceForSpecificContract(int contractId, DateTime? inputEndDate = null);
    }
}
