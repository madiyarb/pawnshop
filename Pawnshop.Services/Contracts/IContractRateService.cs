using System;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Penalty;

namespace Pawnshop.Services.Contracts
{
    public interface IContractRateService
    {
        ContractRate GetPenyAccountRate(int contractId, DateTime date);
        ContractRate GetPenyProfitRate(int contractId, DateTime date);
        ContractRate GetPenyAccountRateWithoutBankRate(int contractId, DateTime date);
        ContractRate GetPenyProfitRateWithoutBankRate(int contractId, DateTime date);
        ContractRate GetRateOnDateByContractAndRateSettingId(int contractId, int RateSettingId, DateTime date);
        IEnumerable<ContractRate> FindRateOnDateByFloatingContractAndRateSettingId(int contractId);
        void DeleteAndInsert(List<ContractRate> contractRates, bool? isFloatingDiscrete);
        void DeleteContractRateForCancelAction(int actionId);
        List<ContractRate> GetLastTwoRatesOnDateByContractAndRateSettingId(int contractId, int RateSettingId, DateTime date);
    }
}