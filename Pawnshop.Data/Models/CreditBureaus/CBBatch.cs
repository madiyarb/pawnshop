using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Data.Models.CreditBureaus;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Отправка данных в кредитные бюро
    /// </summary>
    public class CBBatch : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор компании
        /// </summary>
        [Required(ErrorMessage = "Поле Идентификатор компании обязательно для заполнения")]
        public int OrganizationId { get; set; }

        /// <summary>
        /// Идентификатор КБ
        /// </summary>
        [Required(ErrorMessage = "Поле Идентификатор КБ обязательно для заполнения")]
        public CBType CBId { get; set; }

        /// <summary>
        /// Учетная дата пакета
        /// </summary>
        [Required(ErrorMessage = "Поле Дата пакета обязательно для заполнения")]
        public DateTime BatchDate { get; set; }

        /// <summary>
        /// Идентификатор пакета в КБ
        /// </summary>
        public int? BatchId { get; set; }

        /// <summary>
        /// Идентификатор состояния пакета
        /// </summary>
        public CBBatchStatus BatchStatusId { get; set; }

        /// <summary>
        /// Инфо о статусе пакета
        /// </summary>
        public string BatchStatusInfo { get; set; }

        /// <summary>
        /// Идентификатор состояния пакета
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        public int Size { get; set; }
        public int StartsFrom { get; set; }
        public string FileName { get; set; }

        /// <summary>
        /// Идентификатор файла в ГКБ
        /// </summary>
        public int? FileId { get; set; }

        /// <summary>
        /// Признак для старых или новых договоров. Старые - 3, Новые - 4
        /// </summary>
        public int SchemaId { get; set; }

    }
}