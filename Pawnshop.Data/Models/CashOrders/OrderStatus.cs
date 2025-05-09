using System.ComponentModel;

namespace Pawnshop.Data.Models.CashOrders
{
    /// <summary>
    /// Статус кассового ордера
    /// </summary>
    public enum OrderStatus : short
    {
        /// <summary>
        /// Ожидает подтверждения
        /// </summary>
        [Description("Ожидает подтверждения")]
        WaitingForApprove = 0,
        
        /// <summary>
        /// Ожидает согласования
        /// </summary>
        [Description("Ожидает согласования")]
        WaitingForConfirmation = 1,
        /// <summary>
        /// Согласован
        /// </summary>
        [Description("Согласован")]
        Confirmed = 5,
        
        /// <summary>
        /// Подтвержден
        /// </summary>
        [Description("Подтвержден")]
        Approved = 10,
        
        /// <summary>
        /// Отклонен
        /// </summary>
        [Description("Отклонен")]
        Prohibited = 20
    }
}