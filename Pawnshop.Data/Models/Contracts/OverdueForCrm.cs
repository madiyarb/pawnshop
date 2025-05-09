using System;

namespace Pawnshop.Data.Models.Contracts
{
    public class OverdueForCrm
    {
        /// <summary>
        /// Параметр шины <b><u>uin</u></b>
        /// </summary>
        public string IdentityNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tel</u></b>
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>contract_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_date</u></b>
        /// </summary>
        public DateTime NextPaymentDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_main</u></b>
        /// </summary>
        public decimal DebtCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>procent</u></b>
        /// </summary>
        public decimal PercentCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>penalties</u></b>
        /// </summary>
        public decimal PenaltyCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>value</u></b>
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>current</u></b> (true - 1, false - 0)
        /// </summary>
        public bool ExpiredToday { get; set; }
    }
}
