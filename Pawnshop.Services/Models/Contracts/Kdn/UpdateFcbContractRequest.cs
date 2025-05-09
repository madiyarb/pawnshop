using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class UpdateFcbContractRequest
    {
        public int Id { get; set; }
        public int? FileRowId { get; set; }
        public bool IsLoanPaid { get; set; }
    }
}
