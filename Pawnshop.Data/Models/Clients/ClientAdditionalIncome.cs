using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Доп доход клиента
    /// </summary>
    public class ClientAdditionalIncome : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Тип дохода
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Количество дохода
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ClientAdditionalIncome other = (ClientAdditionalIncome)obj;
            return Id == other.Id &&
                   ClientId == other.ClientId &&
                   TypeId == other.TypeId &&
                   Amount == other.Amount;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ClientId, TypeId, Amount);
        }

    }
}
