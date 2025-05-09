using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Models
{
    public class SendToParking
    {
        public int ContractId { get; set; }
        public int AuthorId { get; set; }

        public static explicit operator HCActionHistory(SendToParking model)
        {
            return new HCActionHistory()
            {
                ActionId = (int)HardCollectionActionTypeEnum.MoveToParkingCar,
                ActionName = HardCollectionActionTypeEnum.MoveToParkingCar.ToString(),
                AuthorId = model.AuthorId,
                Comment = string.Empty,
                CreateDate = DateTime.Now
            };
        }
    }
}
