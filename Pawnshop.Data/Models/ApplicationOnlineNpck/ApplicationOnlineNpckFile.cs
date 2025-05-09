using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using System;

namespace Pawnshop.Data.Models.ApplicationOnlineNpck
{
    public class ApplicationOnlineNpckFile : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public Guid ApplicationOnlineFileId { get; set; }
        public Guid NpckFileId { get; set; }
        public string FileUrl { get; set; }
        public Guid? FutureFileStorageId { get; set; }
        public ApplicationOnlineFile ApplicationOnlineFile { get; set; }


        public ApplicationOnlineNpckFile() { }

        public ApplicationOnlineNpckFile(
            Guid applicationOnlineFileId,
            Guid npckFileId,
            Guid? futureFileStorageId = null,
            string fileUrl = null)
        {
            ApplicationOnlineFileId = applicationOnlineFileId;
            NpckFileId = npckFileId;
            FutureFileStorageId = futureFileStorageId;
            FileUrl = fileUrl;
        }
    }
}
