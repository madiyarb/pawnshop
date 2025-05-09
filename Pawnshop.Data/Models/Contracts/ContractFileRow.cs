using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractFileRow : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        [RequiredId(ErrorMessage = "Поле договор обязательно для заполнения")]
        public int ContractId { get;set; }

        /// <summary>
        /// Действие
        /// </summary>
        public int? ActionId { get; set; }

        /// <summary>
        /// Файл
        /// </summary>
        [RequiredId(ErrorMessage = "Поле файл обязательно для заполнения")]
        public int FileRowId { get;set; }
    }
}