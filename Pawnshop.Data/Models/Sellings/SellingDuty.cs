using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.Sellings
{
    public class SellingDuty
    {
        [Required(ErrorMessage = "Id Реализации обязательно к заполнению")]
        public int Id { get; set; }

        [CustomValidation(typeof(SellingDuty), "SellingCostValidate")]
        [Required(ErrorMessage = "Сумма продажи обязательна к заполнению")]
        public decimal SellingCost { get; set; }

        public static ValidationResult SellingCostValidate(decimal value)
        {
            if (value == 0)
                return new ValidationResult("Сумма продажи обязательна к заполнению");

            return ValidationResult.Success;
        }
    }
}
