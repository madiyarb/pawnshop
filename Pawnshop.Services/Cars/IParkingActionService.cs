using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Parking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Cars
{
    public interface IParkingActionService
    {
        ParkingHistory BuyoutNewParkingHistory(Contract contract);
        Task CancelParkingHistory(Contract contract);
        void ChangeParkingStatusByCategory(Contract contract, int actionId);
        void UpdateParkingHistory(int authorId, bool isChecked, int parentActionId, Contract contract);
    }
}
