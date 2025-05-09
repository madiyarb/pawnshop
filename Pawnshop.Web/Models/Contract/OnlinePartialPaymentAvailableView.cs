namespace Pawnshop.Web.Models.Contract
{
    public class OnlinePartialPaymentAvailableView
    {
        /// <summary>
        /// Признак возможности ЧДП
        /// </summary>
        public bool CanBeProcessed { get; set; }

        /// <summary>
        /// Описание результата
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Максимальная сумма ЧДП
        /// </summary>
        public decimal MaxAmount { get; set; }

        /// <summary>
        /// Минимальная сумма ЧДП
        /// </summary>
        public decimal MinAmount { get; set; }

        /// <summary>
        /// Признак технической проблемы
        /// </summary>
        public bool TechnicalIssues { get; set; }


        public OnlinePartialPaymentAvailableView(bool canBeProcessed, string description, bool technicalIssues = false)
        {
            CanBeProcessed = canBeProcessed;
            Description = description;
            TechnicalIssues = technicalIssues;
        }

        public OnlinePartialPaymentAvailableView(bool canBeProcessed, string description, decimal maxAmount, decimal minAmount, bool technicalIssues = false)
        {
            CanBeProcessed = canBeProcessed;
            Description = description;
            MaxAmount = maxAmount;
            MinAmount = minAmount;
            TechnicalIssues = technicalIssues;
        }
    }
}
