using System;

namespace Pawnshop.Web.Models.ClientRequisites.Bindings
{
    public sealed class CardBinding
    {
        public string CardNumber { get; set; }

        public string HolderName { get; set; }

        public DateTime? ExpireDate { get; set; }

        public Guid FileId { get; set; }
    }
}
