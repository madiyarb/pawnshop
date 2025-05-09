namespace Pawnshop.Services.CardTopUp.StartTransaction
{
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
    public partial class startTransactionResponseReturn
    {

        private ulong customerReferenceField;

        private object errorDescriptionField;

        private string redirectURLField;

        private bool successField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd")]
        public ulong customerReference
        {
            get
            {
                return customerReferenceField;
            }
            set
            {
                customerReferenceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd", IsNullable = true)]
        public object errorDescription
        {
            get
            {
                return errorDescriptionField;
            }
            set
            {
                errorDescriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd")]
        public string redirectURL
        {
            get
            {
                return redirectURLField;
            }
            set
            {
                redirectURLField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd")]
        public bool success
        {
            get
            {
                return successField;
            }
            set
            {
                successField = value;
            }
        }
    }
}
