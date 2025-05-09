using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.AnswerModels
{
    public class AnswerContractDataModel
    {
        public AnswerLoanModel loan { get; set; }
        public AnswerClientModel client { get; set; }
        public AnswerPledgeModel pledge { get; set; }
        public AnswerPaymentScheduleModel[] payment_schedule { get; set; }
        public AnswerPaymentSummaryModel payment_summary { get; set; }
        public object[] documents { get; set; }
        public object[] agreements { get; set; }
        public AnswerAprCalculationDataModel apr_calculation_data { get; set; }
    }
}
