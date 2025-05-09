using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Services.Models.Clients
{
    public class SaveClientAdditionalIncomesRequest
    {
        [Required]
        public List<ClientAdditionalIncomeDto> AdditionalIncomes { get; set; }
    }
}
