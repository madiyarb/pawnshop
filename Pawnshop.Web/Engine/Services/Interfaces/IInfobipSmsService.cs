using Infobip.Api.Model;
using Infobip.Api.Model.Sms.Mt.Reports;
using Infobip.Api.Model.Sms.Mt.Send;
using Pawnshop.Data.Models.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IInfobipSmsService
    {
        Task<SMSResponse> SendSmsAsync(IEnumerable<(string, int)> recepientsWithNotificationReceiverId, string message);
        Task<SMSReportResponse> GetSmsDeliveryReportsAsync(int limit, string messageId = null, string buildId = null);
        NotificationStatus GetNotificationStatusByStatus(Status smsStatus);
        string TruncatePhoneNumber(string number);
    }
}
