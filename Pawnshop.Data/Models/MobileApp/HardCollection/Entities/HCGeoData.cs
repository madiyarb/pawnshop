using Pawnshop.Core;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Entities
{
    public class HCGeoData : IEntity
    {
        public int Id { get; set; }
        public int HCActionHistoryId { get; set; }
        public string GpsCoordinates { get; set; }
        public string AddressText { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }

        public static explicit operator HCGeoDataVM(HCGeoData data)
        {
            return new HCGeoDataVM()
            {
                Id = data.Id,
                HistoryId = data.HCActionHistoryId,
                GpsCoordinates = data.GpsCoordinates,
                AddressText = data.AddressText,
                CreateDate = data.CreateDate
            };
        }
    }
}
