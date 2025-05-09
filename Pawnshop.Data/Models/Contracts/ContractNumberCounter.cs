using Pawnshop.Core;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Счетчик номеров договора
    /// </summary>
    public class ContractNumberCounter : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Год
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Код филиала
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Счетчик
        /// </summary>
        public int Counter { get; set; }
    }
}