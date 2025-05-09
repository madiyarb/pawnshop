using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class ClientDocumentType : IDictionary
    {
        public int Id { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Название на казахском
        /// </summary>
        public string NameKaz { get; set; }
        

        public string Code { get; set; }
        /// <summary>
        /// Признак доступности
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// Для физических/юридических лиц
        /// </summary>
        public bool IsIndividual { get; set; }
        
        /// <summary>
        /// Документ имеет серию
        /// </summary>
        public bool HasSeries { get; set; }

        /// <summary>
        /// Отображаемое наименование поля "Номер документа"
        /// </summary>
        public string NumberPlaceholder { get; set; }
        /// <summary>
        /// Отображаемое наименование поля "Серия документа"
        /// </summary>
        public string SeriesPlaceholder { get; set; }
        /// <summary>
        /// Отображаемое наименование поля "Кем выдан документ"
        /// </summary>
        public string ProviderPlaceholder { get; set; }
        /// <summary>
        /// Отображаемое наименование поля "Место рождения/регистрации"
        /// </summary>
        public string BirthPlacePlaceholder { get; set; }
        /// <summary>
        /// Отображаемое наименование поля "Дата выдачи документа"
        /// </summary>
        public string DatePlaceholder { get; set; }
        /// <summary>
        /// Отображаемое наименование поля "Срок действия документа"
        /// </summary>
        public string DateExpirePlaceholder { get; set; } 

        /// <summary>
        /// Регулярное выражение для проверки номера документа
        /// </summary>
        public string NumberMask { get; set; }
        public string NumberMaskError { get; set; }

        /// <summary>
        /// Идентификатор в КБ
        /// </summary>
        public int? CBId { get; set; }
    }
}
