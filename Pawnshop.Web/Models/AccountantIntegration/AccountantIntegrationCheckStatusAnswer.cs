using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Pawnshop.Web.Models.AccountantIntegration
{
	[XmlRoot(ElementName = "return", Namespace = "http://localhost/wsPayment")]
	public class Return
	{
		[XmlAttribute(AttributeName = "xs", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Xs { get; set; }
		[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "GetStatusResponse", Namespace = "http://localhost/wsPayment")]
	public class GetStatusResponse
	{
		[XmlElement(ElementName = "return", Namespace = "http://localhost/wsPayment")]
		public Return Return { get; set; }
		[XmlAttribute(AttributeName = "m", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string M { get; set; }
	}

	[XmlRoot(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	public class Body
	{
		[XmlElement(ElementName = "GetStatusResponse", Namespace = "http://localhost/wsPayment")]
		public GetStatusResponse GetStatusResponse { get; set; }
	}

	[XmlRoot(ElementName = "Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	public class AccountantIntegrationCheckStatusAnswer
	{
		[XmlElement(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		public Body Body { get; set; }
		[XmlAttribute(AttributeName = "soap", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Soap { get; set; }
	}


}
