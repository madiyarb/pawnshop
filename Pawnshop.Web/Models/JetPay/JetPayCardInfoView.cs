namespace Pawnshop.Web.Models.JetPay
{
    public class JetPayCardInfoView
    {
        public string Portal { get; set; }
        public int Amount { get; set; }
        public string Sender { get; set; }
        public string CardNumberHidden { get; set; }
        public string CardHolderNameHidden { get; set; }
    }
}
