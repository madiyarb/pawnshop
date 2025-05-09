using System;

namespace Pawnshop.Services.Sms
{
    public class SMSInfoTechReportResponseModel
    {
        public string? bulk_id { get; set; }

        public int message_id { get; set; }

        public int? extra_id { get; set; }

        public string? to { get; set; }

        public string? sender { get; set; }

        public string? test { get; set; }

        public DateTime? sent_at { get; set; }

        public DateTime? done_at { get; set; }

        public int? sms_count { get; set; }

        public int? priority { get; set; }

        public string? callback_data { get; set; }

        public string? status { get; set; }

        public int? mnc { get; set; }

        public string? err { get; set; }
    }
}
