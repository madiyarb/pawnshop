using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients
{
    public class VerificateDefaultNumberRequest
    {
        [Required]
        public int? VerificationId { get; set; }
        [Required]
        public int? ClientId { get; set; }
        [Required]
        public string OTP { get; set; }
    }
}
