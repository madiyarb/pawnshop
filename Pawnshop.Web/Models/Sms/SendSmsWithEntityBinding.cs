using System;

namespace Pawnshop.Web.Models.Sms
{
    public class SendSmsWithEntityBinding
    {
        public Guid? ApplicationOnlineId { get; set; }
        public int? ClientId { get; set; }
        public int? InteractionId { get; set; }
        public string Message { get; set; }
        public string PhoneNumber { get; set; }
    }
}
