using Pawnshop.Core;

namespace Pawnshop.Data.Models.Sms
{
    public class SmsCreateNotificationModel
    {
        public int? ContractId { get; set; }
        public int ClientId { get; set; }
        public int BranchId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsNonSchedule { get; set; }
        public bool IsPrivate { get; set; }
        public int AuthorId { get; set; }

        public SmsCreateNotificationModel(
            int clientId,
            int branchId,
            string subject,
            string message,
            int? contractId = null,
            bool isNonSchedule = false,
            bool isPrivate = false,
            int authorId = Constants.ADMINISTRATOR_IDENTITY)
        {
            ContractId = contractId;
            ClientId = clientId;
            BranchId = branchId;
            Subject = subject;
            Message = message;
            IsNonSchedule = isNonSchedule;
            IsPrivate = isPrivate;
            ContractId = contractId;
            AuthorId = authorId;
        }
    }
}
