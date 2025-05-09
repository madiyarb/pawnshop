namespace Pawnshop.Data.Models.ApplicationOnlineRejectionReasons.Bindings
{
    public sealed class ApplicationOnlineRejectionReasonUpdatingBinding
    {
        public string Code { get; set; }

        public string InternalReason { get; set; }

        public string ExternalReasonEn { get; set; }

        public string ExternalReasonKz { get; set; }

        public string ExternalReasonRu { get; set; }

        public bool AvailableToChoiceForClient { get; set; }

        public bool AvailableToChoiceForManager { get; set; }
        public bool Enabled { get; set; }
    }
}
