using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries.PrintTemplates
{
    public class PrintTemplateCounterConfig : IEntity, ILoggableToEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор шаблона
        /// </summary>
        [RequiredId(ErrorMessage = "Идентификатор шаблона обязателен к заполнению")]
        public int TemplateId { get; set; }
        /// <summary>
        /// Формат номера
        /// </summary>
        public string NumberFormat { get; set; }
        /// <summary>
        /// Новый в каждой организации
        /// </summary>
        public bool RelatesOnOrganization { get; set; } = false;
        /// <summary>
        /// Новый в каждом филиале
        /// </summary>
        public bool RelatesOnBranch { get; set; } = false;
        /// <summary>
        /// Новый на каждом виде залога
        /// </summary>
        public bool RelatesOnCollateralType { get; set; } = false;
        /// <summary>
        /// Новый на каждом виде продукта
        /// </summary>
        public bool RelatesOnProductType { get; set; } = false;
        /// <summary>
        /// Новый каждый год
        /// </summary>
        public bool RelatesOnYear { get; set; } = false;
        /// <summary>
        /// Новый в каждом виде начисления процентов
        /// </summary>
        public bool RelatesOnScheduleType { get; set; } = false;
        /// <summary>
        /// Начинать отсчет с
        /// </summary>
        public int BeginFrom { get; set; } = 0;

        public int GetLinkedEntityId()
        {
            return TemplateId;
        }
    }
}
