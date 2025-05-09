using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Restructuring
{
    public class RestructuringDataModel
    {
        /// <summary>
        /// график платежей договора
        /// </summary>
        public List<ContractPaymentSchedule> ContractPaymentSchedule { get; set; }
        /// <summary>
        /// график платежей до реструктуризаций
        /// </summary>
        public List<ContractPaymentSchedule> S1 { get; set; }
        /// <summary>
        /// не просроченный график платежей до реструктуризаций
        /// </summary>
        public List<ContractPaymentSchedule> S1Paid { get; set; }
        /// <summary>
        /// просроченный график платежей до реструктуризаций
        /// </summary>
        public List<RestructuredContractPaymentSchedule> S1Expired { get; set; }
        /// <summary>
        /// график платежей отсрочки
        /// </summary>
        public List<RestructuredContractPaymentSchedule> S2 { get; set; }
        /// <summary>
        /// график платежей реструктуризаций с прибавленными платежами
        /// </summary>
        public List<RestructuredContractPaymentSchedule> S3 { get; set; }
        /// <summary>
        /// первый платеж в графике отсрочки    
        /// </summary>
        public ContractPaymentSchedule futurePaymentScheduleItem { get; set; }
        public LoanPercentSetting Setting { get; set; }
        /// <summary>
        /// ОД который надо реструктуризовать
        /// </summary>
        public decimal DebtLeft { get; set; }
        /// <summary>
        /// Процент который начислен до начала отсрочки
        /// </summary>
        public decimal PercentToAccural { get; set; }
    }
}
