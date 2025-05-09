using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmUploadContract : IEntity
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int UserId { get; set; }
        public int? ContractCrmId { get; set; }
        public int? ClientCrmId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UploadDate { get; set; }
        public int? BitrixId { get; set; }
    }
}