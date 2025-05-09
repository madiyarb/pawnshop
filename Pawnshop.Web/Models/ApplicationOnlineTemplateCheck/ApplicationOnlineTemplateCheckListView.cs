using Pawnshop.Web.Models.Page;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.ApplicationOnlineTemplateCheck
{
    public class ApplicationOnlineTemplateCheckListView : BasePageResponse
    {
        public IEnumerable<ApplicationOnlineTemplateCheckView> List { get; set; }
    }
}
