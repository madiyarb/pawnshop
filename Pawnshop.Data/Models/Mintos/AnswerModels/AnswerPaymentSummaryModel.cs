using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.AnswerModels
{
    public class AnswerPaymentSummaryModel
    {
        public string outstanding_principal { get; set; }
        public string next_payment_total { get; set; }
        public string next_payment_principal { get; set; }
        public string next_payment_interest { get; set; }
        public string next_payment_delayed_interest { get; set; }
        public string next_payment_late_payment_fee { get; set; }
    }
}
