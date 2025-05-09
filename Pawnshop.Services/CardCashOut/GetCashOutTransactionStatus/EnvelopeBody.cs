namespace Pawnshop.Services.CardCashOut.GetCashOutTransactionStatus
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public partial class EnvelopeBody
    {

        private getCashOutTransactionStatusResponse getCashOutTransactionStatusResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
        public getCashOutTransactionStatusResponse getCashOutTransactionStatusResponse
        {
            get
            {
                return this.getCashOutTransactionStatusResponseField;
            }
            set
            {
                this.getCashOutTransactionStatusResponseField = value;
            }
        }
    }
}
