using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.PrintFormInfo
{
    public sealed class PrintFormOpenCreditLineQuestionnaire
    {
        public PrintFormOpenCreditLineQuestionnaireClientInfo ClientInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireClientDocumentInfo DocumentInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireClientContactInfo ContactInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireClientAddressInfo AddressInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireFamilyInfo FamilyInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireEmploymentInfo EmploymentInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireIncomeInfo IncomeInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireConditionInfo ConditionInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireCollateralInfo CollateralInfo { get; set; }
        public PrintFormOpenCreditLineQuestionnaireEstimationInfo EstimationInfo { get; set; }
        public IEnumerable<PrintFormOpenCreditLineQuestionnaireAdditionalContacts> AdditionalContacts { get; set; }
        public PrintFormOpenCreditLineQuestionnaireExpenseInfo ExpenseInfo { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
