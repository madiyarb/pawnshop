using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationOnlineFiles.Views
{
    public sealed class ApplicationOnlineFileListView
    {
        public int Count { get; set; }

        public List<ApplicationOnlineFileView> Files { get; set; }
    }
}
