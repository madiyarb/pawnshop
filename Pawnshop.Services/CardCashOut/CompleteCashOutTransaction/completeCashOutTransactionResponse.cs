namespace Pawnshop.Services.CardCashOut.CompleteCashOutTransaction
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true,
        Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://kz.processing.cnp.merchant_ws/xsd",
        IsNullable = false)]
    public partial class completeCashOutTransactionResponse
    {

        private completeCashOutTransactionResponseReturn returnField;

        /// <remarks/>
        public completeCashOutTransactionResponseReturn @return
        {
            get { return this.returnField; }
            set { this.returnField = value; }
        }
    }
}