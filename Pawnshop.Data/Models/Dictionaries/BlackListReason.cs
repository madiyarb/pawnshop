using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Причина добавления пользователя в черный список
    /// </summary>
    public class BlackListReason : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Поле наименование обязательно для заполнения")]
        public string Name { get; set; }

        /// <summary>
        /// Позволяет создание новых договоров
        /// </summary>
        public bool AllowNewContracts { get; set; }
        /// <summary>
        /// Выдача с правом вождения
        /// </summary>
        public bool AllowNewContractsWithDrive { get; set; }
        /// <summary>
        /// Добор с правам вождения
        /// </summary>
        public bool AdditionNewContractWithDrive { get; set; }
        /// <summary>
        /// ЧДП с правом вождения(смена категорий)
        /// </summary>
        public bool PartialPaymentWithDrive { get; set; }

        /// <summary>
        /// Признак обязательности вложения файла при включении клиента в Черный список
        /// </summary>
        public bool MustHaveAddedFile { get; set; }

        /// <summary>
        /// Признак обязательности вложения файла при исключении клиента из Черного списка
        /// </summary>
        public bool MustHaveRemovedFile { get; set; }

        /// <summary>
        /// Отображение на договоре
        /// </summary>
        public bool IsDisplayed { get; set; }

        /// <summary>
        ///  Тип причины постановки в черный список
        /// </summary>    
        public ReasonType ReasonType { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
