using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Verification
{
    public class GetVerificationRequest
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        [Required]
        public int? ClientId { get; set; }

        /// <summary>
        /// Телефонный номер(опционален, при первой инициализации верификации)
        /// </summary>
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Поле 'телефон' не является мобильным номером")]
        public string PhoneNumber { get; set; }
    }
}
