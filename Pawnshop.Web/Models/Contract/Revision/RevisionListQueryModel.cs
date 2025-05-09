using System;

namespace Pawnshop.Web.Models.Contract.Revision
{
    public class RevisionListQueryModel
    {
        public DateTime? RevisionDate { get; set; }
        public string Status { get; set; }
        public int ContractId { get; set; }
    }
}