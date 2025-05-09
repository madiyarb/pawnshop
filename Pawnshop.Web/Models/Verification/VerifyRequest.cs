using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Verification
{
    public class VerifyRequest
    {
        [Required]
        public string OTP { get; set; }
        [Required]
        public int? ClientId { get; set; }
    }
}
