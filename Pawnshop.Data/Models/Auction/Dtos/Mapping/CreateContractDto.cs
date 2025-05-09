using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Auction.Dtos.Mapping
{
    public class CreateContractDto
    {
        public int ExternalContractId { get; set; }
        public string ContractNumber { get; set; }
        public string Branch { get; set; }
    }
}
