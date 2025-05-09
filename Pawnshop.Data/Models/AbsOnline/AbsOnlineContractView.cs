using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    public class AbsOnlineContractView
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
        /// Параметр шины <b><u>pay_date</u></b> (дата следующего платежа)
        /// </summary>
        public DateTime? NextPaymentDate { get; set; }

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
        /// Параметр шины <b><u>type</u></b> (тип позиции залога)
        /// </summary>
        public string PositionType { get; set; }

        /// <summary>
        /// Параметр шины <b><u>branch_code</u></b> (код филиала)
        /// </summary>
        public string BranchCode { get; set; }

        /// <summary>
        /// Параметр шины <b><u>branch_name</u></b> (наименование филиала)
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Параметр шины <b><u>id</u></b> (идентификатор займа)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Параметр шины <b><u>contract_base_id</u></b> (идентификатор кредитной линии)
        /// </summary>
        public int? CreditLineId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>contract_base</u></b> (номер кредитной линии)
        /// </summary>
        public string CreditLineNumber { get; set; }
    }
}
