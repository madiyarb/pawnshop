namespace Pawnshop.Data.Models.PrintFormInfo
{
    public sealed class PrintFormOpenCreditLineQuestionnaireEstimationInfo
    {
        public decimal EvaluatedAmount { get; set; }

        public PrintFormOpenCreditLineQuestionnaireEstimationInfo(ApplicationsOnlineEstimation.ApplicationsOnlineEstimation estimation)
        {
            EvaluatedAmount = estimation.EvaluatedAmount ?? 0;
        }
    }
}
