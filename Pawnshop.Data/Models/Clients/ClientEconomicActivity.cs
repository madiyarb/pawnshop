using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Перечень ОКЭД клиента
    /// </summary>
    public class ClientEconomicActivity : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        public Client Client { get; set; }
        /// <summary>
        /// Вид ОКЭД
        /// </summary>
        public int EconomicActivityTypeId { get; set; }

        public ClientEconomicActivityType EconomicActivityType { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Идентификатор автора
        /// </summary>
        public int AuthorId { get; set; }

        public User User { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Тип ОКЭД
        /// </summary>
        public int ValueKindId { get; set; }
    }
}