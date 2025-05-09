namespace Pawnshop.Services.CardTopUp.StartTransaction
{
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true,
        Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.w3.org/2003/05/soap-envelope",
        IsNullable = false)]
    public partial class Envelope
    {

        private object headerField;

        private EnvelopeBody bodyField;

        /// <remarks/>
        public object Header
        {
            get { return headerField; }
            set { headerField = value; }
        }

        /// <remarks/>
        public EnvelopeBody Body
        {
            get { return bodyField; }
            set { bodyField = value; }
        }
    }
}
