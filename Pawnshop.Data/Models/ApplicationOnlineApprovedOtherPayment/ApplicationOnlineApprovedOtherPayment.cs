using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.ApplicationOnlineApprovedOtherPayment
{
    public class ApplicationOnlineApprovedOtherPayment : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        [JsonIgnore]
        public User Author { get; set; }
        public DateTime? DeleteDate { get; set; }
        public Guid ApplicationOnlineId { get; set; }
        public string SubjectName { get; set; }
        public decimal Amount { get; set; }
        public Guid FileId { get; set; }
        [JsonIgnore]
        public ApplicationOnlineFile File { get; set; }
    }
}
