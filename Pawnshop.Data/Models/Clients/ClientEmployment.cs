using Pawnshop.Core;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Место работы клиента
    /// </summary>
    public class ClientEmployment : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Флаг основной работы
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Количество работников
        /// </summary>
        public int EmployeeCountId { get; set; }

        /// <summary>
        /// Номер телефона
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Род деятельности
        /// </summary>
        public int BusinessScopeId { get; set; }

        /// <summary>
        /// Опыт работы
        /// </summary>
        public int WorkExperienceId { get; set; }

        /// <summary>
        /// Должность
        /// </summary>
        public string PositionName { get; set; }

        /// <summary>
        /// Тип должности
        /// </summary>
        public int PositionTypeId { get; set; }

        /// <summary>
        /// Доход
        /// </summary>
        public int Income { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
