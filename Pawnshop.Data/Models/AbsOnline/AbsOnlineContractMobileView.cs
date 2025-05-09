using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    public sealed class AbsOnlineContractMobileView
    {
        /// <summary>
        /// Параметр шины <b><u>contract_id</u></b> (номер займа)
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>date_open</u></b> (дата открытия)
        /// </summary>
        public DateTime DateOpen { get; set; }

        /// <summary>
        /// Параметр шины <b><u>date_close</u></b> (дата закрытия)
        /// </summary>
        public DateTime DateClose { get; set; }

        /// <summary>
        /// Параметр шины <b><u>value</u></b> (сумма контракта)
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car</u></b> (наименование автомобиля залога)
        /// </summary>
        public string Car { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_number</u></b> (номерной знак (гос.номер))
        /// </summary>
        public string CarNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>percent</u></b> (процентная ставка (месячная))
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>have_cdp</u></b> (имеет ЧДП)
        /// </summary>
        public bool HasPartialPayment { get; set; }

        /// <summary>
        /// Параметр шины <b><u>contract_type</u></b> (тип займа)
        /// </summary>
        public string ContractType { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_main</u></b> (основной долг)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? PrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_percent</u></b> (проценты)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Profit { get; set; }

        /// <summary>
        /// Параметр шины <b><u>penalties</u></b> (пени)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Penalty { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_all_value</u></b> (сумма для погашения текущей задолженности)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? RepaymentAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_small_value</u></b> (сумма просроченной задолженности и пени)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? OverdueDebt { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_day_expired</u></b> (количество дней просрочки)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PaymentExpiredDays { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_current</u></b> (текущая задолженность)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? DebtCurrent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_date</u></b> (дата следующего платежа)
        /// </summary>
        public DateTime NextPaymentDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_value</u></b> (сумма следующего платежа)
        /// </summary>
        public decimal NextPaymentAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>account_balance</u></b> (сумма переплаты)
        /// </summary>
        public decimal AccountBalance { get; set; }

        /// <summary>
        /// Параметр шины <b><u>balance</u></b> (сумма переплаты)
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_count</u></b> (количество оплаченных платежей)
        /// </summary>
        public int PaidPaymentsCount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_count_expired</u></b> (количество пропущенных платежей)
        /// </summary>
        public int ExpiredPaymentsCount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_count_all</u></b> (количество планируемых платежей)
        /// </summary>
        public int PaymentsCount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_code</u></b> (код продукта)
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_name</u></b> (наименование продукта)
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Параметр шины <b><u>payment_schedule</u></b> (просроченные и предстоящие платежи)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<AbsOnlinePaymentScheduleViewModel> OverdueAndUpcomingPayments { get; set; }

        /// <summary>
        /// Параметр шины <b><u>past_payments</u></b> (оплаченные платежи)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<AbsOnlinePaymentScheduleViewModel> RepaidPayments { get; set; }
    }
}
