using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.ApplicationOnlineNpck
{
    public class ApplicationOnlineNpckSign : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public string SignUrl { get; set; }
        public bool IsSigned { get; set; }
        public string Code { get; set; }


        public ApplicationOnlineNpckSign() { }

        public ApplicationOnlineNpckSign(Guid applicationOnlineId, string signUrl)
        {
            ApplicationOnlineId = applicationOnlineId;
            SignUrl = signUrl;
        }

        public void Sign()
        {
            IsSigned = true;
        }
    }
}
