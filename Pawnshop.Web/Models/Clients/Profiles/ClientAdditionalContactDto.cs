using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Web.Models.Clients.Profiles
{
    public class ClientAdditionalContactDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле 'Номер телефона дополнительного контакта' обязательно для заполнения")]
        [RegularExpression(Constants.KAZAKHSTAN_PHONE_REGEX, ErrorMessage = "Значение поля 'Номер телефона дополнительного контакта' не является телефонным номером")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Поле 'Кем приходится владелец дополнительного контакта' обязательно для заполнения")]
        public int? ContactOwnerTypeId { get; set; }

        [Required(ErrorMessage = "Поле 'ФИО владельца дополнительного контакта' обязательно для заполнения")]
        public string ContactOwnerFullname { get; set; }

        public bool FromContactList { get; set; }

        public string ContactListName { get; set; }

        public bool IsMainPayer { get; set; } = false;
    }
}
