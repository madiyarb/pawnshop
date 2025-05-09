namespace Pawnshop.Services.CreditLines
{
    public sealed class RefillableAccountsInfo
    {
        /// <summary>
        /// Название того что будет погашаться
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Сумма погашения
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Будет ли гаситься полностью или частично
        /// </summary>
        public bool PartialPayment { get; set; } 

        public RefillableAccountsInfo(string name, decimal availableFunds, decimal balance)
        {
            Name = name;
            if (availableFunds >= balance)
            {
                Amount = balance;
                PartialPayment = false;
            }
            else
            {
                Amount = availableFunds;
                PartialPayment = true;
            }
        }

        public RefillableAccountsInfo(string name, decimal amount, bool partialPayment)
        {
            Name = name;
            Amount = amount;
            PartialPayment = partialPayment;
        }
    }
}
