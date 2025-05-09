using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// Расчет ГЭСВ
    /// </summary>
    public class MintosAprCalculationData
    {
        /// <param name="contract">Договор</param>
        public MintosAprCalculationData(Contract contract, decimal exchangeRate)
        {
            net_issued_amount = Math.Round(contract.LoanCost * exchangeRate,2);
            first_agreement_date = contract.ContractDate.ToString("yyyy-MM-dd");
            actual_payment_schedule = new List<MintosActualPaymentScheduleItem>();

            if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
                contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
                contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix)
            {
                foreach (var scheduleItem in contract.PaymentSchedule)
                {
                    MintosActualPaymentScheduleItem item = new MintosActualPaymentScheduleItem
                    {
                        date = scheduleItem.Date.ToString("yyyy-MM-dd"),
                        amount = Math.Round((scheduleItem.DebtCost + scheduleItem.PercentCost) * exchangeRate,2)
                    };

                    actual_payment_schedule.Add(item);
                }
            }
            else if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                MintosActualPaymentScheduleItem item = new MintosActualPaymentScheduleItem
                {
                    date = contract.OriginalMaturityDate.ToString("yyyy-MM-dd"),
                    amount = Math.Round((contract.LoanCost + (contract.LoanPercentCost * contract.LoanPeriod)) * exchangeRate,2)
                };

                actual_payment_schedule.Add(item);
            }
            else throw new ArgumentOutOfRangeException($@"Значение PercentPaymentType={contract.PercentPaymentType} вне диапазона. MintosAprCalculationData.");
        }

        /// <summary>
        /// Сумма ОД договора
        /// </summary>
        public decimal net_issued_amount { get; set; }
        
        /// <summary>
        /// Дата заключения договора
        /// </summary>
        public string first_agreement_date { get; set; }

        /// <summary>
        /// График платежей
        /// </summary>
        public List<MintosActualPaymentScheduleItem> actual_payment_schedule { get; set; }
    }
}
