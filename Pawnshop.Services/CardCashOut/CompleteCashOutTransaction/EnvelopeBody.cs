namespace Pawnshop.Services.CardCashOut.CompleteCashOutTransaction
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true,
        Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public partial class EnvelopeBody
    {

        private completeCashOutTransactionResponse completeCashOutTransactionResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
        public completeCashOutTransactionResponse completeCashOutTransactionResponse
        {
            get { return this.completeCashOutTransactionResponseField; }
            set { this.completeCashOutTransactionResponseField = value; }
        }
    }
}
