using System.Xml.Serialization;

namespace Pawnshop.Services.CardCashOut.StartCashOutTransaction
{
    [XmlRoot(ElementName = "return", Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
    public class Return
    {
        [XmlElement(ElementName = "customerReference", Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd")]
        public string CustomerReference { get; set; }
        [XmlElement(ElementName = "errorDescription", Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd")]
        public string ErrorDescription { get; set; }
        [XmlElement(ElementName = "redirectURL", Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd")]
        public string RedirectURL { get; set; }
        [XmlElement(ElementName = "success", Namespace = "http://beans.merchant_web_services.cnp.processing.kz/xsd")]
        public string Success { get; set; }
        [XmlAttribute(AttributeName = "ax21", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Ax21 { get; set; }
        [XmlAttribute(AttributeName = "ax23", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Ax23 { get; set; }
        [XmlAttribute(AttributeName = "ax26", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Ax26 { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "type", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Type { get; set; }
    }
}
