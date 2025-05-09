using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Sms
{
    public class SmsMessageType : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string EntityType { get; set; }
        public string Title { get; set; }
    }
}
