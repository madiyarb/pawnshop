using System;
using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Services.ApplicationsOnline
{
    public interface IApplicationOnlineCheckCreationService
    {
        public void CreateChecksForManager(Guid applicationOnlineId);

        public void CreateChecksForVerificator(Guid applicationOnlineId);
        public void CreateAutoChecksForBiometric(Guid applicationOnlineId);
        public void CreateAutoChecksForRequisiteValidation(Guid applicationOnlineId);
        public void CreateCheckForIncorrectAmountRequested(Guid applicationOnlineId, decimal requestedAmount,
            decimal totalDebtAmount, decimal creditLineLimit, decimal carCreditLineLimit);

        public void CreateCheckAttentionApplicationAmountChanged(Guid applicationOnlineId, decimal? oldAmount,
            decimal? newAmount);
    }
}
