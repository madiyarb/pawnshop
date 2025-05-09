using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.ClientsGeoPositions.Views
{
    public sealed class ClientGeoPositionListView
    {
        public List<ClientGeoPosition> ClientGeoPositions { get; set; }
        public int Count { get; set; }
    }
}
