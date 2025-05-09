using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Models
{
    public class EventLogNotification
    {
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public EventStatus EventStatus { get; set; }
        public EventCode EventCode { get; set; } = EventCode.MobileAppHardCollectionSendPortfel;
    }
}
