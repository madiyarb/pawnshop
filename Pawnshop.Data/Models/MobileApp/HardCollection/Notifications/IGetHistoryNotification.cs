using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Notifications
{
    public interface IGetHistoryNotification
    {
        HistoryNotification GetHistoryNotification(int? value = null);
    }
}
