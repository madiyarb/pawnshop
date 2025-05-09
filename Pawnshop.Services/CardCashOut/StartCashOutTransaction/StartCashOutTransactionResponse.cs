using System.Xml.Serialization;

namespace Pawnshop.Services.CardCashOut.StartCashOutTransaction
{
    [XmlRoot(ElementName = "startCashOutTransactionResponse", Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
    public class StartCashOutTransactionResponse
    {
        [XmlElement(ElementName = "return", Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
        public Return Return { get; set; }
        [XmlAttribute(AttributeName = "ns", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Ns { get; set; }
    }
}
