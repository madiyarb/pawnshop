using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Verification
{
    public class GetVerificationStatusRequest
    {
        [Required]
        public int? ClientId { get; set; }
        public int? ContractId { get; set; }
    }
}
