using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ReportData
{
    public class UserPermissionReportModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public bool Locked { get; set; }
        public string PermissionName { get; set; }
        public string PermissionDisplayName { get; set; }
    }
}
