using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ClientDeferments
{
    public class ClientDefermentModel
    {
        public string FullName { get; set; }
        public int ClientId { get; set; }
        public string IIN { get; set; }
        public string ClientStatus { get; set; }
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public bool IsContractRestructured { get; set; }
        public string DefermentTypeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

    }
}
