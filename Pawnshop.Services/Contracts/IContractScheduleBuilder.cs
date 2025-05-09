using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.Contracts
{
    [Obsolete("Use IPaymentScheduleService")]
    public interface IContractScheduleBuilder
    {
        [Obsolete]
        List<ContractPaymentSchedule> BuildAnnuity(DateTime beginDate, decimal loanCost, decimal loanPercentPerDay, DateTime maturityDate, DateTime? firstPaymentDate = null, int? paymentsCount = null, int? paymentDebt = null, bool isCreditLine = false, bool ChDP4MigratedFromOnline = false, DateTime? changeDate = null, Contract contract = null, bool isChDP = false);
        [Obsolete]
        List<ContractPaymentSchedule> BuildDiscrete(decimal loanCost, decimal loanPercentPerDay, DateTime maturityDate);
        /// <summary>
        /// Расчет графика погашения платежей по дифференцированному займу
        /// </summary>
        /// <param name="beginDate">Дата выдачи займа</param>
        /// <param name="loanCost">Сумма займа</param>
        /// <param name="loanPercentPerDay">Ежедневный процент вознаграждения</param>
        /// <param name="maturityDate">Предполагаемая дата погашения займа</param>
        /// <param name="firstPaymentDate">Дата первого платежа</param>
        /// <param name="paymentsCount">Количество платежей</param>
        /// <returns>График погашения</returns>
        [Obsolete]
        public List<ContractPaymentSchedule> BuildDifferent(DateTime beginDate, decimal loanCost, decimal loanPercentPerDay, DateTime maturityDate, DateTime? firstPaymentDate = null, int? paymentsCount = null, DateTime? changeDate = null, Contract contract = null, bool isChDP = false);
        [Obsolete]
        Task BuildScheduleForNewContract(Contract contract, bool migratedFromOnline = false, bool isChDP = false, DateTime? changeDate = null, DateTime? actionDate = null);
        [Obsolete]
        List<ContractPaymentSchedule> CalculateScheduleWithoutContract(LoanPercentSetting setting, decimal loanCost, DateTime beginDate, DateTime maturityDate, DateTime? firstPaymentDate = null);
        [Obsolete]
        Task BuildScheduleForNewFloatingContract(Contract contract, bool isChDP = false, DateTime? actionDate = null);
        [Obsolete]
        Task<Contract> SaveBuilderByControlDate(ContractPaymentScheduleUpdateModel contr);
    }
}
