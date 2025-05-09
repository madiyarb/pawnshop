using Pawnshop.Data.Models.Audit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Models
{
    public class FileLogNotification
    {
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public EventStatus EventStatus { get; set; }
    }
}
