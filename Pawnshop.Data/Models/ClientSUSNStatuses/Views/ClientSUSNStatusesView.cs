using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.ClientSUSNStatuses.Views
{
    public sealed class ClientSUSNStatusesView
    {
        public List<ClientSUSNStatusView> List { get; set; }
        public int Count { get; set; }
        public bool AnySUSNStatus { get; set; }
    }
}
