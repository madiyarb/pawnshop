using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Models
{
    public class HistoryNotification : HCActionHistory
    {
        public int ContractId { get; set; }
    }
}
