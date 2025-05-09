using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IMultiCurrency
    {
        [CustomValidation(typeof(IMultiCurrency), "CurrencyValidate")]
        int CurrencyId { get; set; }

        static ValidationResult CurrencyValidate(int value, ValidationContext context)
        {
            if (value <= 0)
            {
                return new ValidationResult($"Валюта обязательна к заполнению");
            }

            return ValidationResult.Success;
        }
    }
}
