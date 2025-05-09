using System;
using Pawnshop.Data.Models.ApplicationOnlineFiles;

namespace Pawnshop.Data.Models.ApplicationOnlineFileLogItems
{
    public class ApplicationOnlineFileLogData
    {
        public Guid ApplicationId { get; set; }
        public Guid FileId { get; set; }
        public Guid ApplicationOnlineFileCode { get; set; }

        public ApplicationOnlineFileLogData()
        {
            
        }

        public ApplicationOnlineFileLogData(ApplicationOnlineFile file)
        {
            ApplicationId = file.ApplicationId;
            FileId = file.Id;
            ApplicationOnlineFileCode = file.ApplicationOnlineFileCodeId;
        }
    }
}
