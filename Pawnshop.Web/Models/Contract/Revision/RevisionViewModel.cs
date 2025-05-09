using System;

namespace Pawnshop.Web.Models.Contract.Revision
{
    public class RevisionViewModel
    {
        public string ContractNumber { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
    }
}