using System;

namespace Pawnshop.Data.Models.ApplicationOnlineFileLogItems.Views
{
    public sealed class ApplicationOnlineFileLogItemView
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid FileId { get; set; }
        public Guid ApplicationOnlineFileCode { get; set; }
        public string Title { get; set; }
    }
}
