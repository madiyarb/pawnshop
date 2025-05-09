using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.AnswerModels
{
    public class AnswerPaymentScheduleModel
    {
        public int number { get; set; }
        public string date { get; set; }
        public string principal_amount { get; set; }
        public string interest_amount { get; set; }
        public string accumulated_interest_amount { get; set; }
        public string delayed_amount { get; set; }
        public string penalties_amount { get; set; }
        public string sum { get; set; }
        public string received_amount { get; set; }
        public bool is_repaid { get; set; }
        public string total_remaining_principal { get; set; }

        public MintosInvestorPaymentScheduleItem ConvertToSaved(int contractId, int? mintosContractId = null)
        {
            var newScheduleItem = new MintosInvestorPaymentScheduleItem();

            newScheduleItem.ContractId = contractId;
            newScheduleItem.Number = number;
            newScheduleItem.SendedDate = newScheduleItem.MintosDate = DateTime.Parse(date);
            newScheduleItem.IsRepaid = is_repaid;
            newScheduleItem.MintosPrincipalAmount = newScheduleItem.SendedPrincipalAmount = Parse(principal_amount);
            newScheduleItem.MintosInterestAmount = newScheduleItem.SendedInterestAmount = Parse(interest_amount);
            newScheduleItem.MintosDelayedAmount = newScheduleItem.SendedDelayedAmount = Parse(delayed_amount);
            newScheduleItem.MintosTotalSum = newScheduleItem.SendedTotalSum = Parse(sum);
            newScheduleItem.MintosTotalRemainingPrincipal = newScheduleItem.SendedTotalRemainingPrincipal = Parse(total_remaining_principal);
            newScheduleItem.Status = is_repaid ? MintosInvestorPaymentStatus.Paid : MintosInvestorPaymentStatus.Await;

            if (mintosContractId.HasValue)
            {
                newScheduleItem.MintosContractId = mintosContractId.Value;
            }

            return newScheduleItem;
        }

        private decimal Parse(string s)
        {
            s = s.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            return decimal.Parse(s, NumberStyles.Any,
                CultureInfo.InvariantCulture);
        }
    }
}
