using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Sms
{
    public class SmsMessageAttribute : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string Attribute { get; set; }
        public string Title { get; set; }
        public int SmsMessageTypeId { get; set; }
        public SmsMessageType SmsMessageType { get; set; }
    }
}
