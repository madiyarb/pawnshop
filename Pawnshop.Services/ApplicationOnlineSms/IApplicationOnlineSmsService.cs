using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineSignOtpVerifications;
using System;

namespace Pawnshop.Services.ApplicationOnlineSms
{
    public interface IApplicationOnlineSmsService
    {
        public ApplicationOnlineSignOtpVerification SendSmsForSign(Guid applicationOnlineId, string? phoneNumber = null);

        public string SendSms(string message, int clientId, string subject, int? branchId,
            string? phoneNumber = null, int userId = Constants.ADMINISTRATOR_IDENTITY);

    }
}
