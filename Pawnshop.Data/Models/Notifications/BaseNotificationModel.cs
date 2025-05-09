namespace Pawnshop.Data.Models.Notifications
{
    public class BaseNotificationModel
    {
        public int? ContractId { get; set; }
        public int ClientId { get; set; }
        public int BranchId { get; set; }
    }
}
