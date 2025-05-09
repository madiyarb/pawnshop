using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Cars.Views
{
    public sealed class OnlineCarInfoView
    {
        [JsonIgnore]
        public int ContractId { get; set; }
        public string Model { get; set; }
        public string TransportNumber { get; set; }
        public string BodyNumber { get; set; }
        public double EstimatedCost { get; set; }
        public string IssueYear { get; set; }
        public int ParkingStatus { get; set; }
        public bool WithRightToDrive { get; set; }

    }
}
