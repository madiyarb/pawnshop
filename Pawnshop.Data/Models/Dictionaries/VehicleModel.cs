using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Счет
    /// </summary>
    public class VehicleModel : IEntity
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

        /// <summary>
        /// Марка 
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Марка обязательно для заполнения")]
        public int VehicleMarkId { get; set; }

        public VehicleMark VehicleMark { get; set; }
        
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Информация по ликвидности
        /// </summary>
        public List<VehicleLiquidity>? VehicleLiquidities { get; set; }

        public int AuthorId { get; set; }
    }
}