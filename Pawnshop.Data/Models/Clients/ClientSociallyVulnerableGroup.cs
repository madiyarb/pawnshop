using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Список социально уязвимых слоев населения
    /// </summary>
    public class ClientSociallyVulnerableGroup : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Запись из социально уязвимых слоев населения
        /// </summary>
        public int SociallyVulnerableGroupId { get; set; }

        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}