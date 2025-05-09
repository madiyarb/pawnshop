using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class FcbContractsExistsRequest
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public int SubjectTypeId { get; set; }
    }
}
