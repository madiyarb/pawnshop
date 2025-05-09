using System;

namespace Pawnshop.Data.Models.SUSNStatuses.Views
{
    public sealed class SUSNStatusView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string Name { get; set; }
        public string NameKz { get; set; }
        public string Code { get; set; }
        public bool? Permanent { get; set; }
        public bool? Decline { get; set; }
    }
}
