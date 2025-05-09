using System;

namespace Pawnshop.Web.Models.AbsOnlineCardCashOut
{
    public sealed class CardCashoutTransactionView
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
        /// Номер карты клиента на которую производится вывод денег
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Имя владельца карты
        /// </summary>
        public string CardHolderName { get; set; }
        /// <summary>
        /// Сумма вывода в Тиынах
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
        /// Guid используется для ввода данных карты с бека  
        /// </summary>
        public string TranGUID { get; set; }

        /// <summary>
        /// Текущий статус транзакции
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Время создания записи
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Время удаления записи
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
