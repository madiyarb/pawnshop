using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.UploadModels
{
    public class MintosPaymentUploadModel
    {
        public MintosPaymentUploadModel(int number, DateTime date, string principal_amount, string interest_amount, string penalty_amount, string total_remaining_principal)
        {
            this.number = number;
            this.date = date.Date.ToString("yyyy-MM-dd");
            this.principal_amount = principal_amount;
            this.interest_amount = interest_amount;
            this.penalty_amount = penalty_amount;
            this.total_remaining_principal = total_remaining_principal;
        }

        public int number { get; set; }
        public string date { get; set; }
        public string principal_amount { get; set; }
        public string interest_amount { get; set; }
        public string penalty_amount { get; set; }
        public string total_remaining_principal { get; set; }
    }
}
