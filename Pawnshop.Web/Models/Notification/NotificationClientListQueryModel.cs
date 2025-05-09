using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Models.Notification
{
    public class NotificationClientListQueryModel
    {
        public int NotificationId { get; set; }

        public int BranchId { get; set; }

        public CollateralType? CollateralType { get; set; }

        public CardType? CardType { get; set; }
    }
}
