using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ApplicationOnlineInsurances
{
    [Table("ApplicationOnlineInsurances")]
    public class ApplicationOnlineInsurance
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public string Status { get; set; }
        public decimal InsurancePremium { get; set; }
        public decimal AmountForCustomer { get; set; }
        public decimal TotalLoanAmount { get; set; }

        public ApplicationOnlineInsurance()
        {

        }

        public ApplicationOnlineInsurance(Guid id, Guid applicationOnlineId,
            decimal insurancePremium, decimal amountForCustomer, decimal totalLoanAmount)
        {
            Id = id;
            CreateDate = DateTime.Now;
            ApplicationOnlineId = applicationOnlineId;
            Status = ApplicationOnlineInsuranceStatus.IsProvided.ToString();
            InsurancePremium = insurancePremium;
            AmountForCustomer = amountForCustomer;
            TotalLoanAmount = totalLoanAmount;
        }

        public void Delete()
        {
            DeleteDate = DateTime.Now;
            Status = ApplicationOnlineInsuranceStatus.Deleted.ToString();
        }
    }
}
