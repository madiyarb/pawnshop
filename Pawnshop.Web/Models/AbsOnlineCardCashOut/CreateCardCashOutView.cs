namespace Pawnshop.Web.Models.AbsOnlineCardCashOut
{
    public class CreateCardCashOutView
    {
        public string Url { get; set; }
        public string CustomerReference { get; set; }
        public string Portal { get; set; }
        public int Amount { get; set; }
        public string Sender { get; set; }
        public string CardNumberHidden { get; set; }
        public string CardHolderNameHidden { get; set; }
        public string ReferenceNr { get; set; }
        public string TranGuid { get; set; }
    }
}
