using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.AnswerModels
{
    public class AnswerPledgeModel
    {
        public string type { get; set; }
        public object appraiser { get; set; }
        public string valuation { get; set; }
        public string valuation_date { get; set; }
        public string city { get; set; }
        public string country_iso_code { get; set; }
        public object body_type { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public int? year { get; set; }
        public object fuel_type { get; set; }
        public object displacement { get; set; }
        public AnswerDateModel first_registration_date { get; set; }
    }
}
