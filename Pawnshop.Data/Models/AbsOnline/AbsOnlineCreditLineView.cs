using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    public class AbsOnlineCreditLineView
    {
        /// <summary>
        /// Параметр шины <b><u>credit_line</u></b> (номер контракта)
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>rest</u></b> (остаток суммы)
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>start_date</u></b> (дата открытия кредитной линии)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>end_date</u></b> (дата окончания)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_number</u></b> (номерной знак (гос.номер))
        /// </summary>
        public string CarNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_vin</u></b> (VIN)
        /// </summary>
        public string CarVin { get; set; }

        /// <summary>
        /// Параметр шины <b><u>limit_sum</u></b> (сумма кредитной линии)
        /// </summary>
        public decimal CreditLineLimit { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_od</u></b> (основной долг по КЛ)
        /// </summary>
        public decimal PrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_main</u></b> (сумма выкупа по кредитной линии)
        /// </summary>
        public decimal RedemptionAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_day_expired</u></b> (кол-во дней просрочки по КЛ)
        /// </summary>
        public int PaymentExpiredDays { get; set; }

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
        /// Параметр шины <b><u>balance</u></b> (остаток на авансовом счете)
        /// </summary>
        public decimal PrepaymentBalance { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_percent</u></b> (начисленные проценты по траншам КЛ)
        /// </summary>
        public decimal ProfitAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_fine</u></b> (сумма пени и госпошлины и другие законные платежи не являющиеся погашением процентов и ОД)
        /// </summary>
        public decimal PenyAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_brand</u></b> (марка авто)
        /// </summary>
        public string CarBrand { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_model</u></b> (модель авто)
        /// </summary>
        public string CarModel { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_code</u></b> (код продукта)
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_name</u></b> (наименование продукта)
        /// </summary>
        public string ProductName { get; set; }
    }
}
