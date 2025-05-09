using Pawnshop.Data.Models.Sms;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.Sms
{
    public class SmsTemplateListView
    {
        public int Count { get; set; }
        public List<SmsTemplateView> List { get; set; } = new List<SmsTemplateView>();
    }
}
