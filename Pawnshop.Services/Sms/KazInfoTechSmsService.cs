using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.MessageSenders;
using RestSharp;

namespace Pawnshop.Services.Sms
{
    public class KazInfoTechSmsService : IKazInfoTechSmsService
    {

        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;

        public KazInfoTechSmsService(IOptions<EnviromentAccessOptions> options, OuterServiceSettingRepository outerServiceSettingRepository)
        {
            EnviromentAccessOptions envOptions = options.Value;
            _outerServiceSettingRepository = outerServiceSettingRepository;
        }

        public SMSInfoTechResponseModel SendSMS(string message, MessageReceiver receiver)
        {
            string controllerURL = _outerServiceSettingRepository.Find(new {Code = Constants.SMS_INTEGRATION_SERVICE_SETTING_CODE}).ControllerURL;
            RestClient client = new RestClient($"{controllerURL}/api/SMS/Send");
            RestRequest request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            var param = new SMSSendModel ( message, receiver);
            request.AddJsonBody(param);
            var response = client.Execute(request);
            SMSInfoTechResponseModel responseModel = JsonConvert.DeserializeObject<SMSInfoTechResponseModel>(response.Content);
            return responseModel;
        }

        public string GetStatusMessage(SMSInfoTechResponseModel response)
        {
            return JsonConvert.SerializeObject(response); 
        }

        public NotificationStatus GetStatus(SMSInfoTechResponseModel response)
        {
            if (response.status.Equals("sent") || response.status.Equals("sending") || response.status.Equals("send"))
            {
                return NotificationStatus.Sent;
            }
            return NotificationStatus.ForSend;
        }

        public NotificationStatus GetReportStatus(SMSInfoTechReportResponseModel report)
        {
            if (report.status.Equals("sent") || report.status.Equals("sending") || report.status.Equals("send"))
            {
                return NotificationStatus.Sent;
            }if(report.status.Equals("delivered"))
            {
                return NotificationStatus.Delivered;
            }
            return NotificationStatus.NotDelivered;
        }

        public SMSInfoTechReportResponseModel GetReport(int messageId, RestClient? client = null)
        {
            string controllerURL = _outerServiceSettingRepository.Find(new { Code = Constants.SMS_INTEGRATION_SERVICE_SETTING_CODE }).ControllerURL;
            SMSInfoTechReportResponseModel reportResponse = null;
            string URL = $"{controllerURL}/api/SMS/GetReport";

            client ??= new RestClient(URL);

            RestRequest request = new RestRequest(Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("messageId", messageId, ParameterType.QueryString);

            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                reportResponse = JsonConvert.DeserializeObject<SMSInfoTechReportResponseModel>(response.Content);
            }

            return reportResponse;
        }

    }
}
