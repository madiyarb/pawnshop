using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ApplicationOnlineRejectionReasons.Bindings
{
    public sealed class ApplicationOnlineRejectionReasonCreationBinding
    {
        public string Code { get; set; }

        public string InternalReason { get; set; }

        public string ExternalReasonEn { get; set; }

        public string ExternalReasonKz { get; set; }

        public string ExternalReasonRu { get; set; }

        public bool AvailableToChoiseForClient { get; set; }

        public bool AvailableToChoiseForManager { get; set; }
    }
}
