namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class ContractRowsDto
    {
        /// <summary>
        /// Основной долг. PaymentType: Debt
        /// </summary>
        public decimal Debt { get; set; }
        
        /// <summary>
        /// Просроченный основной долг. PaymentType: OverdueDebt
        /// </summary>
        public decimal OverdueDebt { get; set; }
        
        /// <summary>
        /// Просроченное вознаграждение. PaymentType: Loan
        /// </summary>
        public decimal Loan { get; set; }
        
        /// <summary>
        /// Вознаграждение. PaymentType: OverdueLoan
        /// </summary>
        public decimal OverdueLoan { get; set; }
        
        /// <summary>
        /// Штраф/пеня на основной долг. PaymentType: DebtPenalty
        /// </summary>
        public decimal DebtPenalty { get; set; }
        
        /// <summary>
        /// Штраф/пеня на вознаграждение/проценты. PaymentType: LoanPenalty
        /// </summary>
        public decimal LoanPenalty { get; set; }
    }
}