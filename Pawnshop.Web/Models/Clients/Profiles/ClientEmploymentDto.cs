using Pawnshop.Core;
using Pawnshop.Core.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients.Profiles
{
    public class ClientEmploymentDto
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }

        [Required(ErrorMessage = "Поле 'Название места работы' обязательно для заполнения")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле 'Количество работников места работы' обязательно для заполнения")]
        public int? EmployeeCountId { get; set; }

        [Required(ErrorMessage = "Значение поля 'Телефон места работы' обязательно для заполнения")]
        [RegularExpression(Constants.KAZAKHSTAN_PHONE_REGEX, ErrorMessage = "Значение поля 'Телефон места работы' не является телефонным номером")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Значение поля 'Адрес места работы' обязательно для заполнения")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Значение поля 'Род деятельности места работы' обязательно для заполнения")]
        public int? BusinessScopeId { get; set; }

        [Required(ErrorMessage = "Значение поля 'Стаж работы на месте работы' обязательно для заполнения")]
        public int? WorkExperienceId { get; set; }

        [Required(ErrorMessage = "Значение поля 'Название должности на месте работы' обязательно для заполнения")]
        public string PositionName { get; set; }

        [Required(ErrorMessage = "Значение поля 'Тип должности на месте работы' обязательно для заполнения")]
        public int? PositionTypeId { get; set; }

        //[Required(ErrorMessage = "Значение поля 'Доход на месте работы' обязательно для заполнения")]
        //[Range(1, int.MaxValue, ErrorMessage = "Поле 'Доход' должен быть положительным числом")]
        //public int? Income { get; set; }
    }
}
