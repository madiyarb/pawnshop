using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Pawnshop.Web.Models.NationalBankExchangeRates
{
    public class RatesXmlParser
    {
        public Rate Parse(string xml)
        {
            if (string.IsNullOrEmpty(xml)) throw new ArgumentException($"The '{nameof(xml)}' cannot be null or empty.");

            XmlSerializer serializer = new XmlSerializer(typeof(Rate), new XmlRootAttribute("rates"));
            using (TextReader reader = new StringReader(xml))
            {
                Rate rate = (Rate)serializer.Deserialize(reader);
                return rate;
            }
        }
    }
}
