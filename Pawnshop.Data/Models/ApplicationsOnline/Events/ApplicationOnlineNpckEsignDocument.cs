using Pawnshop.Data.Models.ApplicationsOnline.Views;
using System.Collections.Generic;
using System;

namespace Pawnshop.Data.Models.ApplicationsOnline.Events
{
    public class ApplicationOnlineNpckEsignDocument
    {
        public string Token { get; set; }
        public int ListId { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public List<ApplicationOnlineNpckFileView> FilesInfo { get; set; }
    }
}
