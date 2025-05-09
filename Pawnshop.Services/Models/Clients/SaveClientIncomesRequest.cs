using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Services.Models.Clients
{
    public class SaveClientIncomesRequest
    {
        [Required]
        public List<ClientIncomeDto> ClientIncomes { get; set; }
    }
}
