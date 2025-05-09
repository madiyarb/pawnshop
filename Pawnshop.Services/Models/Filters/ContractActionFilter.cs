using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Models.Filters
{
    public class ContractActionFilter
    {
        public int? ContractId { get; set; }
        public ContractActionType? ActionType { get; set; }
    }
}
