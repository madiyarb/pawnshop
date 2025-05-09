using System;
using System.Collections.Generic;

namespace Pawnshop.Services.Clients
{
    public interface IClientExpiredSchedulesGetterService
    {
        public int? Calculate(int clientId);
        public List<ExpiredPaymentSchedule> GetAllExpiredPaymentSchedules(int clientId);

        public bool SomeCurrentContractsOnExpiredNow(int clientId);
    }

    public sealed class ExpiredPaymentSchedule
    {
        public DateTime PaymentDate { get; set; }
        public DateTime? RealPaymentDate { get; set; }
        public string ContractNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal MainDebt { get; set; }
        public decimal Percent { get; set; }
        public decimal? Penalty { get; set; }
        public int ExpiredDays { get; set; }
        public bool IsPayed { get; set; }
    }
}
