namespace Pawnshop.Web.Models.Sms
{
    public class SmsTemplateCreateBinding
    {
        public int? SmsMessageTypeId { get; set; }
        public string Title { get; set; }
        public int? ManualSendRoleId { get; set; }
        public string MessageTemplate { get; set; }
    }
}
