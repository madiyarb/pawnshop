using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Models.Contract
{
    public class ContractQueryModel
    {
        /// <summary>
        /// Позиция
        /// </summary>
        public int PositionId { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType? CollateralType { get; set; }

        /// <summary>
        /// Статус договора
        /// </summary>
        public ContractStatus Status { get; set; }
    }
}
