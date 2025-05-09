using System;

namespace Pawnshop.Data.Models.PrintFormInfo
{
    public sealed class PrintFormOpenCreditLineQuestionnaireClientDocumentInfo
    {
        public DateTime DocumentDate { get; set; }

        public string DocumentNumber { get; set; }

        public string DocumentProviderRu { get; set; }

        public string DocumentProviderKz { get; set; }

        public DateTime DateExpire { get; set; }
    }
}
