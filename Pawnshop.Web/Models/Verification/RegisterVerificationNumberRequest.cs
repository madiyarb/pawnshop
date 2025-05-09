using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Verification
{
    public class RegisterVerificationNumberRequest
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        [Required]
        public int? ClientId { get; set; }

        /// <summary>
        /// Номер телефона 
        /// </summary>
        [Required]
        public string PhoneNumber { get; set; }
    }
}
