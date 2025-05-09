using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.AnswerModels
{
    public class AnswerAprCalculationDataModel
    {
        public string net_issued_amount { get; set; }
        public string annual_percentage_rate { get; set; }
        public string first_agreement_date { get; set; }
        public AnswerActualPaymentScheduleModel[] actual_payment_schedule { get; set; }
    }
}
