using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Services.Models.Vehicle
{
    public class VehicleModelDto
    {
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

        public DateTime? DeleteDate { get; set; }

        public VehicleLiquidity VehicleLiquidity { get; set; }
    }
}
