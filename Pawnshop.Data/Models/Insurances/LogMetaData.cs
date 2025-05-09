using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Insurances
{
    public class LogMetaData
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? ContractId { get; set; }
        public EntityType EntityType { get; set; }
        public string Token { get; set; } = String.Empty;
    }
}
