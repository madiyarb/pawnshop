using Pawnshop.Web.Models.Page;

namespace Pawnshop.Web.Models.Sms
{
    public class SmsTemplateListQuery : PageSettingFromQuery
    {
        public string EntityType { get; set; }
        public int? ManualSendRoleId { get; set; }
    }
}
