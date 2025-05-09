using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.UploadModels
{
    public class MintosExtendScheduleModel
    {
        public List<ExtendPaymentSchedule> payment_schedule { get; set; } = new List<ExtendPaymentSchedule>();
    }

    public class ExtendPaymentSchedule
    {
        public int number { get; set; }
        public string date { get; set; }
    }

}
