using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Models.Notification
{
    public class NotificationPeriodQueryModel : Data.Models.Notifications.Notification
    {
        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Период
        /// </summary>
        public int Period { get; set; }
    }
}
