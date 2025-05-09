using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmSyncContact : IEntity
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? ClientCrmId { get; set; }
        public int? CrmId { get; set; }
        public String CrmName { get; set; }
        public String CrmIdentityNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UploadDate { get; set; }
    }
}