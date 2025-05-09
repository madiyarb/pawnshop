using System;
namespace Pawnshop.Data.Models.MobileApp.HardCollection.Models
{
    public class HCGeoDataVM
    {
        public int Id { get; set; }
        public int HistoryId { get; set; }
        public string GpsCoordinates { get; set; }
        public string AddressText { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
