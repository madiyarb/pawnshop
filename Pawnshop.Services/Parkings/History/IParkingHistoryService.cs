using Pawnshop.Data.Models.Parking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Parkings.History
{
    public interface IParkingHistoryService
    {
        ParkingHistory Save(ParkingHistory parkingHistory);
    }
}
