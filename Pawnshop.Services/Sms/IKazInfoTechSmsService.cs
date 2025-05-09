using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.MessageSenders;
using RestSharp;

namespace Pawnshop.Services.Sms
{
    public interface IKazInfoTechSmsService
    {

        SMSInfoTechResponseModel SendSMS(string message, MessageReceiver receiver);

        string GetStatusMessage(SMSInfoTechResponseModel response);

        NotificationStatus GetStatus(SMSInfoTechResponseModel response);
        SMSInfoTechReportResponseModel GetReport(int messageId, RestClient? client = null);

        public NotificationStatus GetReportStatus(SMSInfoTechReportResponseModel report);
    }
}
