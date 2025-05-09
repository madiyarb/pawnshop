using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.LoanSettings;

namespace Pawnshop.Data.Models.Dictionaries.PrintTemplate
{
    public class PrintTemplateCounterFilter
    {

        /// <summary>
        /// Идентификатор настройки
        /// </summary>
        public int ConfigId { get; set; }

        /// <summary>
        /// Огранизация
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType? CollateralType { get; set; }

        /// <summary>
        /// Вид продукта
        /// </summary>
        public int? ProductTypeId { get; set; }

        /// <summary>
        /// Год
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Вид начисления(графика)
        /// </summary>
        public ScheduleType? ScheduleType { get; set; }
    }
}
