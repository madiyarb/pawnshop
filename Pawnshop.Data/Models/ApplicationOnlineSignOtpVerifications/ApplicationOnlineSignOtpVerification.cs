using System;

namespace Pawnshop.Data.Models.ApplicationOnlineSignOtpVerifications
{
    public sealed class ApplicationOnlineSignOtpVerification
    {
        public Guid Id { get; set; }

        public Guid ApplicationOnlineId { get; set; }

        public string Code { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public int RetryCount { get; set; }

        public int TryCount { get; set; }
        public string PhoneNumber { get; set; }
        public bool? Success { get; set; }

        public ApplicationOnlineSignOtpVerification() { }

        public ApplicationOnlineSignOtpVerification(Guid id, Guid applicationId, string code, int retryCount, int tryCount, string phoneNumber)
        {
            Id = id;
            ApplicationOnlineId = applicationId;
            Code = code;
            RetryCount = retryCount;
            TryCount = tryCount;
            PhoneNumber = phoneNumber;
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }

        public void Verify(string code)
        {
            if (Code == code)
                Success = true;
            else
                Success = false;
            TryCount++;
            UpdateDate = DateTime.Now;
        }
    }
}
