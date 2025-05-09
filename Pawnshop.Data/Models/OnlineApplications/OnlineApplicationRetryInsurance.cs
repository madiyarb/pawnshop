using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.OnlineApplications
{
    public class OnlineApplicationRetryInsurance : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int ContractId { get; set; }
        public bool IsSuccessful { get; set; }
        public int Attempts { get; set; }
    }
}
