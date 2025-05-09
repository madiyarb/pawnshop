using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Models
{
    public class LogNotification
    {
        public Exception Exception { get; set; }
        public EventLogNotification EventLogNotification { get; set; }
        public TelegramLogNotification TelegaLogNotification { get; set; }
        public FileLogNotification FileLogNotification { get; set; }
    }
}
