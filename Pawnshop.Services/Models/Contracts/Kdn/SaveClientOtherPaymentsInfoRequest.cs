using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class SaveClientOtherPaymentsInfoRequest
    {
        [Required]
        public List<ContractKdnDetailDto> clientOtherPaymentsInfo { get; set; }
    }
}
