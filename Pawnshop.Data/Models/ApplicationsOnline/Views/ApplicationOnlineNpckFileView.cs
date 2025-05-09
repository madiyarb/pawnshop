using System;

namespace Pawnshop.Data.Models.ApplicationsOnline.Views
{
    public class ApplicationOnlineNpckFileView
    {
        public Guid NpckFileId { get; set; }
        public Guid FileStorageId { get; set; }


        public ApplicationOnlineNpckFileView() { }

        public ApplicationOnlineNpckFileView(Guid npckFileId, Guid fileStorageId)
        {
            NpckFileId = npckFileId;
            FileStorageId = fileStorageId;
        }
    }
}
