using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// Договор
    /// </summary>
    public class MintosLoan
    {
        /// <param name="contract">Договор</param>
        /// <param name="interestRate">Ставка для инвестора</param>
        /// <param name="currency">Валюта</param>
        /// <param name="exchangeRate">Обменный курс, по отношению к валюте договора(по умолчанию = 1)</param>
        /// <param name="parentContract">Родительский договор</param>
        public MintosLoan(Contract contract, decimal interestRate, string currency, decimal exchangeRate = 1, Contract parentContract = null)
        {
            var leftLoanCost = contract.LeftLoanCost;

            lender_id = parentContract == null
                ? contract.ContractNumber
                : String.Concat(parentContract.ContractNumber, "_", contract.ContractNumber);
            lender_issue_date = contract.ContractDate.ToString("yyyy-MM-dd");
            loan_amount = Math.Round(contract.LoanCost * exchangeRate,2);
            loan_amount_assigned_to_mintos = Math.Round(leftLoanCost * exchangeRate,2);
            interest_rate_percent = interestRate;
            this.currency = currency;
            currency_exchange_rate = exchangeRate;

            if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
                contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
                contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix)
            {
                final_payment_date = contract.PaymentSchedule.OrderByDescending(x => x.Date).FirstOrDefault().Date.ToString("yyyy-MM-dd");
                prepaid_schedule_payments = contract.PaymentSchedule.Where(x => x.ActionId != null).Count();
            }
            else if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                final_payment_date = contract.MaturityDate.ToString("yyyy-MM-dd");
                prepaid_schedule_payments = contract.Actions.Where(x => x.ActionType == ContractActionType.Prolong && x.Date.Date<DateTime.Now.Date).Count();
            }
            else throw new ArgumentOutOfRangeException($@"Значение PercentPaymentType={contract.PercentPaymentType} вне диапазона. MintosLoan.");
        }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string lender_id { get; set; }

        /// <summary>
        /// Код страны
        /// </summary>
        public string country => "KZ";

        /// <summary>
        /// Дата выдачи
        /// </summary>
        public string lender_issue_date { get; set; }

        /// <summary>
        /// Дата передачи в Mintos
        /// </summary>
        public string mintos_issue_date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        /// <summary>
        /// Дата последней оплаты
        /// </summary>
        public string final_payment_date { get; set; }

        /// <summary>
        /// Количество предоплаченных платежей
        /// </summary>
        public int prepaid_schedule_payments { get; set; }

        /// <summary>
        /// Сумма основного долга договора
        /// </summary>
        public decimal loan_amount { get; set; }

        /// <summary>
        /// Сумма передачи
        /// </summary>
        public decimal loan_amount_assigned_to_mintos { get; set; }

        /// <summary>
        /// Процент для инвестора
        /// </summary>
        public decimal interest_rate_percent { get; set; }

        /// <summary>
        /// Вид расписания, по умолчанию "full"
        /// </summary>
        public string schedule_type => "full";

        /// <summary>
        /// Выкуп обратно через 60 дней
        /// </summary>
        public bool buyback { get; set; } = true;

        /// <summary>
        /// Шаблон договора цессии, по умолчанию "full"
        /// </summary>
        public string cession_contract_template => "default";

        /// <summary>
        /// Валюта
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Обменный курс, по отношению к валюте договора
        /// </summary>
        public decimal currency_exchange_rate { get; set; }
        /// <summary>
        /// График может расширяться
        /// </summary>
        public bool extendable => true;
    }
}
