using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>Ликвидность машины</summary>
    public class VehicleLiquidity : IEntity
    {
        /// <summary>Идентификатор</summary>
        public int Id { get; set; }

        public int VehicleMarkId { get; set; }

        public VehicleMark VehicleMark { get; set; }

        public int VehicleModelId { get; set; }

        public VehicleModel VehicleModel { get; set; }

        /// <summary>Ликвидность по умолчанию</summary>
        [Required(ErrorMessage = "Поле Ликвидность по умолчанию обязательно для заполнения")]
        public LiquidDefaultType LiquidDefault { get; set; }

        /// <summary>Ликвидность по условию</summary>
        public LiquidDefaultType? LiquidByAdditionCondition { get; set; }

        /// <summary>Год в котором измениться ликвидность</summary>
        public int? YearCondition { get; set; }

        /// <summary>Идентификатор автора</summary>
        public int AuthorId { get; set; }

        /// <summary>Дата удаления</summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>Дата создания</summary>
        public DateTime CreateDate { get; set; }
    }
}