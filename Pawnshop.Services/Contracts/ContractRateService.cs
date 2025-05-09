using System;
using System.Collections.Generic;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Services.Contracts
{
    public class ContractRateService : IContractRateService
    {
        private readonly ContractRateRepository _contractRateRepository;
        public ContractRateService(ContractRateRepository contractRateRepository)
        {
            _contractRateRepository = contractRateRepository;
        }

        public ContractRate GetPenyAccountRate(int contractId, DateTime date) 
            => _contractRateRepository.FindRateOnDateByContractAndCode(contractId, Constants.ACCOUNT_SETTING_PENY_ACCOUNT, date);

        public ContractRate GetPenyProfitRate(int contractId, DateTime date) 
            => _contractRateRepository.FindRateOnDateByContractAndCode(contractId, Constants.ACCOUNT_SETTING_PENY_PROFIT, date);

        public ContractRate GetPenyAccountRateWithoutBankRate(int contractId, DateTime date)
            => _contractRateRepository.FindRateOnDateByContractAndCodeWithoutBankRate(contractId, Constants.ACCOUNT_SETTING_PENY_ACCOUNT, date);

        public ContractRate GetPenyProfitRateWithoutBankRate(int contractId, DateTime date)
            => _contractRateRepository.FindRateOnDateByContractAndCodeWithoutBankRate(contractId, Constants.ACCOUNT_SETTING_PENY_PROFIT, date);

        public ContractRate GetRateOnDateByContractAndRateSettingId(int contractId, int RateSettingId, DateTime date)
            => _contractRateRepository.FindRateOnDateByContractAndRateSettingId(contractId, RateSettingId, date);    
        
        public IEnumerable<ContractRate> FindRateOnDateByFloatingContractAndRateSettingId(int contractId)
            => _contractRateRepository.FindRateOnDateByFloatingContractAndRateSettingId(contractId);

        public void DeleteAndInsert(List<ContractRate> contractRates, bool? isFloatingDiscrete)
            => _contractRateRepository.DeleteAndInsert(contractRates, isFloatingDiscrete);

        public void DeleteContractRateForCancelAction(int actionId)
            => _contractRateRepository.DeleteContractRateForCancelAction(actionId);

        public List<ContractRate> GetLastTwoRatesOnDateByContractAndRateSettingId(int contractId, int RateSettingId, DateTime date)
            => _contractRateRepository.FindLastTwoRatesOnDateByContractAndRateSettingId(contractId, RateSettingId, date);
    }
}