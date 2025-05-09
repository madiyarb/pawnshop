using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients.Profiles
{
    public class ClientProfileDto
    {
        public int? EducationTypeId { get; set; }
        public int? TotalWorkExperienceId { get; set; }
        public int? MaritalStatusId { get; set; }
        public string SpouseFullname { get; set; }
        public int? SpouseIncome { get; set; }
        public int? ChildrenCount { get; set; }
        public int? AdultDependentsCount { get; set; }
        public int? UnderageDependentsCount { get; set; }
        public int? ResidenceAddressTypeId { get; set; }
        public bool? IsWorkingNow { get; set; }
        public bool? HasAssets { get; set; }
    }
}
