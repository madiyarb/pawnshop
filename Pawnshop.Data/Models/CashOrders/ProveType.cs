namespace Pawnshop.Data.Models.CashOrders
{
    /// <summary>
    /// Статус кассового ордера
    /// </summary>
    public enum ProveType : short
    {
        /// <summary>
        /// Документы имеются
        /// </summary>
        Proven = 10,
        /// <summary>
        /// Документов нету
        /// </summary>
        NotProven = 20
    }
}