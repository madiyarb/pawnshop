using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.UserPermissionsReport
{
    public class UserPermissionsReportQueryModel
    {
        public List<int> UserIds { get; set; }

        public List<int> RoleIds { get; set; }
    }
}
