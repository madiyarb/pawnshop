using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationOnlineRejectionReasons.Views
{
    public sealed class ApplicationOnlineRejectionReasonListView
    {
        public List<ApplicationOnlineRejectionReason> List { get; set; }

        public int Count { get; set; }
    }
}
