using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Notifications;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmPaymentUpdate : CrmPaymentBaseAction
    {
        public CrmPaymentUpdate(
            Contracts.Contract contract,
            int categoryId,
            List<Notification> notifications,
            string defaultContact,
            decimal loanCostLeft,
            decimal loanPercentCost,
            decimal penaltyPercentCost,
            decimal prepayment,
            decimal buyoutAmount,
            decimal prolongAmount,
            int overdueContracts,
            CollectionContractStatus collectionStatus,
            string prodUrl
            )
            : base(
                contract,
                categoryId,
                notifications,
                defaultContact,
                loanCostLeft,
                loanPercentCost,
                penaltyPercentCost,
                prepayment,
                buyoutAmount,
                prolongAmount,
                overdueContracts,
                collectionStatus,
                prodUrl)
        {
        }
    }
}
