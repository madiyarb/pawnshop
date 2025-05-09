using System;

namespace Pawnshop.Web.Models.Sms
{
    public class SmsMessageAttributeView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Attribute { get; set; }
        public string Title { get; set; }
        public int SmsMessageTypeId { get; set; }
        public string SmsMessageTypeName { get; set; }
    }
}
