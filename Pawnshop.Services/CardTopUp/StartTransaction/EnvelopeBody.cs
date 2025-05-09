namespace Pawnshop.Services.CardTopUp.StartTransaction
{
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public partial class EnvelopeBody
    {

        private startTransactionResponse startTransactionResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
        public startTransactionResponse startTransactionResponse
        {
            get
            {
                return startTransactionResponseField;
            }
            set
            {
                startTransactionResponseField = value;
            }
        }
    }
}
