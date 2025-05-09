using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.ApplicationOnlineNpck
{
    public class ApplicationOnlineNpckSignFile : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid ApplicationOnlineFileId { get; set; }
        public Guid EsignFileId { get; set; }


        public ApplicationOnlineNpckSignFile() { }

        public ApplicationOnlineNpckSignFile(Guid applicationOnlineFileId, Guid esignFileId)
        {
            ApplicationOnlineFileId = applicationOnlineFileId;
            EsignFileId = esignFileId;
            CreateDate = DateTime.Now;
        }
    }
}
