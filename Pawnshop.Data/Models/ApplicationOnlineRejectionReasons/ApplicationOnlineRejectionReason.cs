using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ApplicationOnlineRejectionReasons
{
    [Table("ApplicationOnlineRejectionReasons")]
    public sealed class ApplicationOnlineRejectionReason
    {
        [Key]
        public int Id { get; set; }

        public string Code { get; set; }

        public string InternalReason { get; set; }

        public string ExternalReasonEn { get; set; }

        public string ExternalReasonKz { get; set; }

        public string ExternalReasonRu { get; set; }

        public bool AvailableToChoiceForClient { get; set; }

        public bool AvailableToChoiceForManager { get; set; }

        public bool AutoRejectReason { get; set; }

        public bool Enabled { get; set; }

        public ApplicationOnlineRejectionReason() { }

        public ApplicationOnlineRejectionReason(string code, string internalReason, string externalReasonEn,
            string externalReasonKz, string externalReasonRu, bool availableToChoiceForClient, bool availableToChoiceForManager, bool autoRejectReason = false)
        {
            Code = code;
            InternalReason = internalReason;
            ExternalReasonEn = externalReasonEn;
            ExternalReasonKz = externalReasonKz;
            ExternalReasonRu = externalReasonRu;
            AvailableToChoiceForClient = availableToChoiceForClient;
            AvailableToChoiceForManager = availableToChoiceForManager;
            AutoRejectReason = false;
            Enabled = true;
        }

        public void Update(string code, string internalReason, string externalReasonEn,
            string externalReasonKz, string externalReasonRu, bool availableToChoiseForClient,
            bool availableToChoiseForManager, bool enabled)
        {
            Code = code;
            InternalReason = internalReason;
            ExternalReasonEn = externalReasonEn;
            ExternalReasonKz = externalReasonKz;
            ExternalReasonRu = externalReasonRu;
            AvailableToChoiceForClient = availableToChoiseForClient;
            AvailableToChoiceForManager = availableToChoiseForManager;
            Enabled = enabled;
        }

        public void Disable()
        {
            Enabled = false;
        }
    }
}
