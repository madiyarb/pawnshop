using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Clients
{
    public class ClientFileRow : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        [RequiredId(ErrorMessage = "Поле клиент обязательно для заполнения")]
        public int ClientId { get;set; }

        /// <summary>
        /// Документ
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// Файл
        /// </summary>
        [RequiredId(ErrorMessage = "Поле файл обязательно для заполнения")]
        public int FileRowId { get;set; }
    }
}