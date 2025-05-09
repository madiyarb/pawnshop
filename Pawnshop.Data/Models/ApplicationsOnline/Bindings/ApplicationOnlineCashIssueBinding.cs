namespace Pawnshop.Data.Models.ApplicationsOnline.Bindings
{
    public class ApplicationOnlineCashIssueBinding
    {
        /// <summary>
        /// Признак выдачи через кассу
        /// </summary>
        public bool IsCashIssue { get; set; }

        /// <summary>
        /// Идентификатор филиала для выдачи средств через кассу
        /// </summary>
        public int? CashIssueBranchId { get; set; }
    }
}
