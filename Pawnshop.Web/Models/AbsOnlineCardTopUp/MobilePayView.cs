namespace Pawnshop.Web.Models.AbsOnlineCardTopUp
{
    public sealed class MobilePayView
    {
        public string contract_id { get; set; }
        public string totalAmount { get; set; }
        public string redirectURL { get; set; }
    }
}
