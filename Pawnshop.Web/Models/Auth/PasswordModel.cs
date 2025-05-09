using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Web.Models.Auth
{
    public class PasswordModel
    {
        [Required(ErrorMessage = "Укажите текущий пароль.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Укажите новый пароль.")]
        [CustomValidation(typeof(PasswordModel), "NewPasswordValidate")]
        public string NewPassword { get; set; }

        public static ValidationResult NewPasswordValidate(string value, ValidationContext context)
        {
            var model = (PasswordModel)context.ObjectInstance;

            if (model.OldPassword == value)
                return new ValidationResult("Старый и новый пароль не должны совпадать");

            Regex regex = new Regex(Constants.PASSWORD_REGEX);

            if (!regex.IsMatch(value))
                throw new PawnshopApplicationException(
                    new List<string>()
                    {
                        "Пароль должен быть не меньше 8 символов",
                        "и содержать:",
                        "Хотя бы одну латинскую букву нижнего регистра",
                        "Хотя бы одну латинскую букву верхнего регистра",
                        "Хотя бы одну цифру",
                        "Хотя бы один спецсимвол"
                    }.ToArray());

            return ValidationResult.Success;
        }
    }
}