using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.ApplicationOnlineFcbKdnPayment
{
    public class ApplicationOnlineFcbKdnPayment : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        [JsonIgnore]
        public User Author { get; set; }
        public DateTime? DeleteDate { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public decimal PaymentAmount { get; set; }
        public bool Success { get; set; }
    }
}
