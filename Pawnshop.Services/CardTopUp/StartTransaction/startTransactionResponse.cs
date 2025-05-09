namespace Pawnshop.Services.CardTopUp.StartTransaction
{
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://kz.processing.cnp.merchant_ws/xsd", IsNullable = false)]
    public partial class startTransactionResponse
    {

        private startTransactionResponseReturn returnField;

        /// <remarks/>
        public startTransactionResponseReturn @return
        {
            get
            {
                return returnField;
            }
            set
            {
                returnField = value;
            }
        }
    }
}
