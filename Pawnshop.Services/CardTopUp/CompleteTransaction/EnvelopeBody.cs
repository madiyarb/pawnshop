namespace Pawnshop.Services.CardTopUp.CompleteTransaction
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public partial class EnvelopeBody
    {

        private completeTransactionResponse completeTransactionResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
        public completeTransactionResponse completeTransactionResponse
        {
            get
            {
                return this.completeTransactionResponseField;
            }
            set
            {
                this.completeTransactionResponseField = value;
            }
        }
    }
}
