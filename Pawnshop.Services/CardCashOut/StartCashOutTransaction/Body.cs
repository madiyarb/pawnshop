using System.Xml.Serialization;

namespace Pawnshop.Services.CardCashOut.StartCashOutTransaction
{
    [XmlRoot(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Body
    {
        [XmlElement(ElementName = "startCashOutTransactionResponse", Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
        public StartCashOutTransactionResponse StartCashOutTransactionResponse { get; set; }
    }
}
