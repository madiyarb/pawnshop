using System.Xml.Serialization;

namespace Pawnshop.Services.CardCashOut.StartCashOutTransaction
{
    [XmlRoot(ElementName = "errorDescription", Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd")]
    public class ErrorDescription
    {
        [XmlAttribute(AttributeName = "nil", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Nil { get; set; }
    }

}
