using Pawnshop.Web.Models.Page;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.ApplicationOnlineCheck
{
    public class ApplicationOnlineCheckListView
    {
        public IEnumerable<ApplicationOnlineCheckView> Checks { get; set; }
    }
}
