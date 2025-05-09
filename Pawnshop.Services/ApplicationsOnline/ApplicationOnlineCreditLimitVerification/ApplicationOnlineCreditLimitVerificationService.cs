using System;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.CreditLines;

namespace Pawnshop.Services.ApplicationsOnline.ApplicationOnlineCreditLimitVerification
{
    public sealed class ApplicationOnlineCreditLimitVerificationService : IApplicationOnlineCreditLimitVerificationService
    {
        private readonly ApplicationOnlineInsuranceRepository _applicationOnlineInsuranceRepository;
        private readonly ApplicationOnlinePositionRepository _applicationOnlinePositionRepository;
        private readonly CreditLineService _creditLineService;
        private readonly IApplicationOnlineCheckCreationService _applicationOnlineCheckCreationService;
        private readonly ContractPositionRepository _contractPositionRepository;
        public ApplicationOnlineCreditLimitVerificationService(ApplicationOnlineInsuranceRepository applicationOnlineInsuranceRepository, 
            ApplicationOnlinePositionRepository applicationOnlinePositionRepository,
            CreditLineService creditLineService,
            IApplicationOnlineCheckCreationService applicationOnlineCheckCreationService,
            ContractPositionRepository contractPositionRepository)
        {
            _applicationOnlineInsuranceRepository = applicationOnlineInsuranceRepository;
            _applicationOnlinePositionRepository = applicationOnlinePositionRepository;
            _creditLineService = creditLineService;
            _applicationOnlineCheckCreationService = applicationOnlineCheckCreationService;
            _contractPositionRepository = contractPositionRepository;
        }

        public async Task<ApplicationOnlineCreditLimitVerificationResult> Check(ApplicationOnline application)
        {
            var sums = await GetAmounts(application);
            return new ApplicationOnlineCreditLimitVerificationResult()
            {
                Message = sums.Comment(),
                Result = sums.Validate()
            };
        }

        public async Task ValidateSumsAndCreateCheck(ApplicationOnline application)
        {
            var sums = await GetAmounts(application);
            if (sums.Validate())
                return;
            _applicationOnlineCheckCreationService
                .CreateCheckForIncorrectAmountRequested(application.Id, sums.RequestedAmount,
                sums.CurrentMainDebtAmount, sums.CreditLineLimitAmount, sums.CarCreditLineLimitAmount);
        }

        private async Task<ApplicationOnlineControlSums> GetAmounts(ApplicationOnline application)
        {
            var sums = new ApplicationOnlineControlSums
            {
                CarCreditLineLimitAmount = 0,
                CreditLineLimitAmount = 0,
                CurrentMainDebtAmount = 0,
                RequestedAmount = 0
            };

            if (application.CreditLineId.HasValue)
            {
                var creditLineBalance = (await _contractPositionRepository.GetContractPositionByContractId(application.CreditLineId.Value)).FirstOrDefault()?.LoanCost ?? 0;
                var balances = await _creditLineService.GetCurrentlyDebtForCreditLine(application.CreditLineId.Value);
                sums.CreditLineLimitAmount = creditLineBalance;
                sums.CurrentMainDebtAmount = balances.ContractsBalances.Sum(balance => balance.AccountAmount);
            }
            var insurance = await _applicationOnlineInsuranceRepository.GetByApplicationId(application.Id);
            if (insurance != null)
            {
                sums.RequestedAmount = insurance.TotalLoanAmount;
            }
            else
            {
                sums.RequestedAmount = application.ApplicationAmount;
            }
            var position = _applicationOnlinePositionRepository.Get(application.ApplicationOnlinePositionId);

            if (position != null)
            {
                sums.CarCreditLineLimitAmount = position.LoanCost.Value;
            }

            return sums;
        }

    }
}
