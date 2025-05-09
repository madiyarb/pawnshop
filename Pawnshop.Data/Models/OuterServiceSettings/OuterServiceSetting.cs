using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.OuterServiceSettings
{
    public class OuterServiceSetting : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id компаний предоставляющий внешний сервис
        /// </summary>
        public int? ServiceCompanyId { get; set; }

        /// <summary>
        /// Login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Url внешнего сервиса
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Ссылка на контроллер Spring сервиса
        /// </summary>
        public string ControllerURL { get; set; }

        /// <summary>
        /// Тип авторизации
        /// </summary>
        public int AuthTypeId { get; set; }
        public DomainValue AuthType { get; set; }

        /// <summary>
        /// Тип сервиса
        /// </summary>
        public int? ServiceTypeId { get; set; }
        public DomainValue ServiceType { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата создания обязательно для заполнения")]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
