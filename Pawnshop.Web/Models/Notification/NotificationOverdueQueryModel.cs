using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Models.Notification
{
    public class NotificationOverdueQueryModel : Data.Models.Notifications.Notification
    {
        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }
    }
}
