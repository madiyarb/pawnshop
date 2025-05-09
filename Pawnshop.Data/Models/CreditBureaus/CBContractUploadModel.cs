using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.CreditBureaus
{
    public class CBContractUploadModel
    {
        public int EventLogId { get; set; }
        public int ContractId { get; set; }
        public ContractClass ContractClass { get; set; }
        public int BatchId { get; set; }
        public string ResponseData { get; set; }
        public string BranchName { get; set; }
        public int ClientId { get; set; }
        public string IIN { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
