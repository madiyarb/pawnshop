using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System;

namespace Pawnshop.Data.Models.Sms
{
    public class SmsTemplate : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? SmsMessageTypeId { get; set; }
        public SmsMessageType SmsMessageType { get; set; }
        public string Title { get; set; }
        public int? ManualSendRoleId { get; set; }
        public Role Role { get; set; }
        public string MessageTemplate { get; set; }
    }
}
