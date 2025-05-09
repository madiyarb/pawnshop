using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.CardTopUpTransaction
{
    public sealed class CardTopUpTransaction : IEntity
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Сумма вывода 
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Уникальный идентификатор для идентификации выполнения транзакции в процессинге
        /// </summary>
        public string CustomerReference { get; set; }

        /// <summary>
        /// Url в процессинге по которому должен проходить вввод данных карты клиента
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Время создания записи
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Время обновления записи
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Время удаления записи
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Текущий статус транзакции
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Номер заказа
        /// </summary>
        public int OrderId { get; set; }

    }
}
