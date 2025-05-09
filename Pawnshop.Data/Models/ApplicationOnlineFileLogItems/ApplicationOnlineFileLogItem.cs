using Dapper.Contrib.Extensions;
using System;

namespace Pawnshop.Data.Models.ApplicationOnlineFileLogItems
{
    [Table("ApplicationOnlineFileLogItems")]
    public class ApplicationOnlineFileLogItem : ApplicationOnlineFileLogData
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }

        public ApplicationOnlineFileLogItem(ApplicationOnlineFileLogData data, int? userId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UserId = userId;
            ApplicationId = data.ApplicationId;
            FileId = data.FileId;
            ApplicationOnlineFileCode = data.ApplicationOnlineFileCode;
        }
    }
}
