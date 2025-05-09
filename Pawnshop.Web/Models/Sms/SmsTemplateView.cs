using System;

namespace Pawnshop.Web.Models.Sms
{
    public class SmsTemplateView
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? SmsMessageTypeId { get; set; }
        public string SmsMessageTypeName { get; set; }
        public string Title { get; set; }
        public int? ManualSendRoleId { get; set; }
        public string ManualSendRoleName { get; set; }
        public string MessageTemplate { get; set; }
    }
}
