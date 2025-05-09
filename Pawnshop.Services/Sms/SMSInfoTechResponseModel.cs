namespace Pawnshop.Services.Sms
{
    public class SMSInfoTechResponseModel
    {
        public int? message_id { get; set; }

        public string? to { get; set; }

        public string? status { get; set; }

        public int? sms_count { get; set; }

        public int? error_code { get; set; }

        public string? error_message { get; set; }
    }
}
