using System;
namespace Pawnshop.Web.Models.Contract
{
    /// <summary>
    /// Внешняя информация о договоре
    /// </summary>
    public class ContractOuterModel
    {
        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Дата ближайшего платежа
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Остаток
        /// </summary>
        public int BalanceCost { get; set; }
    }
}
