using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Требуемые аккаунты//настройки аккаунтов
    /// </summary>
    public class LoanProductTypeAccount : IDictionary
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Вид продукта
        /// </summary>
        public int ProductTypeId { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Код
        /// </summary>
        [Required(ErrorMessage = "Поде \"Код\" в настройках аккаунтов для видов продуктов обязательно к заполнению")]
        public string Code { get; set; }
        /// <summary>
        /// Номер счета
        /// </summary>
        [RequiredId(ErrorMessage = "Поде \"Номер счета\" в настройках аккаунтов для видов продуктов обязательно к заполнению")]
        public int AccountId { get; set; }
        /// <summary>
        /// Номер счета
        /// </summary>
        public Account Account { get; set; }//TODO: переделать в план счетов(AccountPlan)
    }
}
