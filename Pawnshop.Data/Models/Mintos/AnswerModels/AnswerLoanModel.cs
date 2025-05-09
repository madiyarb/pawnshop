using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.AnswerModels
{
    public class AnswerLoanModel
    {
        public int mintos_id { get; set; }
        public string public_id { get; set; }
        public string lender_id { get; set; }
        public string country_id { get; set; }
        public AnswerDateModel lender_issue_date { get; set; }
        public AnswerDateModel mintos_issue_date { get; set; }
        public AnswerDateModel final_payment_date { get; set; }
        public int? prepaid_schedule_payments { get; set; }
        public bool buyback { get; set; }
        public string loan_amount { get; set; }
        public string loan_amount_assigned_to_mintos { get; set; }
        public object undiscounted_principal { get; set; }
        public string interest_rate_percent { get; set; }
        public string schedule_type { get; set; }
        public string status { get; set; }
        public object purpose { get; set; }
        public string cession_contract_template { get; set; }
        public string currency_exchange_rate { get; set; }
        public object assigned_origination_fee_share { get; set; }
        public bool extendable { get; set; }
    }
}
