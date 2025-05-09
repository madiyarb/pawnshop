using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class ContractKdnDetailModel
    {
        public int ClientId { get; set; }
        public bool IsSubject { get; set; } = false;
        public string FIO { get; set; }
        public decimal Amount4KdnSum { get; set; }
        public List<ContractKdnDetail> ContractKdnDetails { get; set; }
        public int? PositionEstimatedCost { get; set; }
    }
}
