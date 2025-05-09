using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.AnswerModels
{
    public class AnswerClientModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public int? age { get; set; }
        public object liability { get; set; }
        public object dependants { get; set; }
        public object occupation { get; set; }
        public object income_monthly { get; set; }
        public string personal_identification { get; set; }
        public object contact_person { get; set; }
        public string phone_number { get; set; }
        public string address_street_actual { get; set; }
        public object company_name { get; set; }
        public object registration_number { get; set; }
        public int? legal_type { get; set; }
        public string birth_date { get; set; }
    }
}
