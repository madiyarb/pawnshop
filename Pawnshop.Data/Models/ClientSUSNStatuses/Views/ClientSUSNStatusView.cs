using System;

namespace Pawnshop.Data.Models.ClientSUSNStatuses.Views
{
    public sealed class ClientSUSNStatusView
    {
        public string Name { get; set; }
        public string NameKz { get; set; }
        public string Code { get; set; }
        public bool Permanent { get; set; }
        public bool Decline { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
