using System;
using System.Collections.Generic;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Марка
    /// </summary>
    public class VehicleMark : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }        

        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Поле Наименование обязательно")]
        public string Name { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        [Required(ErrorMessage = "Поле Код обязательно")]
        public string Code { get; set; }
        
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public ICollection<VehicleModel> VehicleModels { get; set; }
    }
}