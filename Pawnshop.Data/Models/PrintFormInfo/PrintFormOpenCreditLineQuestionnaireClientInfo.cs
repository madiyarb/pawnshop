using System;

namespace Pawnshop.Data.Models.PrintFormInfo
{
    public sealed class PrintFormOpenCreditLineQuestionnaireClientInfo
    {
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime BirthDay { get; set; }
        public bool ASPRecipient { get; set; }
        public bool IsPolitician { get; set; }
    }
}
