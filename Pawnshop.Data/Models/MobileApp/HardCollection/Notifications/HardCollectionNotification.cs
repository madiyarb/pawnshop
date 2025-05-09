using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Notifications
{
    public class HardCollectionNotification : INotification
    {
        public HistoryNotification HistoryNotification { get; set; }
        public LogNotification LogNotification { get; set; }
    }
}
