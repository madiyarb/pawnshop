using System;

namespace Pawnshop.Web.Models.SUSNStatuses
{
    public class SUSNStatusBinding
    {
        public string Name { get; set; }
        public string NameKz { get; set; }
        public string Code { get; set; }
        public bool Permanent { get; set; }
        public bool Decline { get; set; }
    }
}
