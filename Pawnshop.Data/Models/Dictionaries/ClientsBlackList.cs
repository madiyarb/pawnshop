using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Основание включения и удаления из черного списка клиентов
    /// </summary>
    public class ClientsBlackList : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public int ClientId { get; set; }

        public Client Client { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public int ReasonId { get; set; }

        public BlackListReason BlackListReason { get; set; }

        /// <summary>
        /// Пользователь добавивший в черный список клиентов
        /// </summary>
        public int? AddedBy { get; set; }

        /// <summary>
        /// Основание добавления в черный список клиентов
        /// </summary>
        public string AddReason { get; set; }

        /// <summary>
        /// Дата добавления клиента в черный список клиентов
        /// </summary>
        public DateTime? AddedAt { get; set; }

        /// <summary>
        /// Пользователь добавивший в черный список клиентов
        /// </summary>
        public int? RemovedBy { get; set; }

        /// <summary>
        /// Основание исключения из черного списка клиентов
        /// </summary>
        public string RemoveReason { get; set; }

        /// <summary>
        /// Дата исключения из черного списка клиентов
        /// </summary>
        public DateTime? RemoveDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Файл для добавления в черный список клиентов
        /// </summary>
        public int? AddedFileRowId { get; set; }

        /// <summary>
        /// Файл для исключения из черного списка клиентов
        /// </summary>
        public int? RemovedFileRowId { get; set; }

        /// <summary>
        /// Файл
        /// </summary>
        public FileRow AddedFile { get; set; }

        /// <summary>
        /// Файл
        /// </summary>
        public FileRow RemovedFile { get; set; }
    }
}
