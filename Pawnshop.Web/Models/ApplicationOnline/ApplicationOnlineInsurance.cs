using System;

namespace Pawnshop.Web.Models.ApplicationOnline
{
    public sealed class ApplicationOnlineInsurance
    {
        public Guid Id { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public string Status { get; set; }
        public decimal InsurancePremium { get; set; }
        public decimal AmountForCustomer { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal Percent { get; set; }

        public ApplicationOnlineInsurance(Guid id, Guid applicationOnlineId, string status, 
            decimal insurancePremium, decimal amountForCustomer, decimal totalLoanAmount, decimal percent)
        {
            Id = id;
            ApplicationOnlineId = applicationOnlineId;
            Status = status;
            InsurancePremium = insurancePremium;
            AmountForCustomer = amountForCustomer;
            TotalLoanAmount = totalLoanAmount;
            Percent = percent;
        }
    }
}
