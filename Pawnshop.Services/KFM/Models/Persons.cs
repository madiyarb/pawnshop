using System.Collections.Generic;
using System.Xml.Serialization;

namespace Pawnshop.Services.KFM.Models
{
    [XmlRoot(ElementName = "persons")]
    public class Persons
    {
        [XmlElement(ElementName = "person")]
        public List<Person> Person { get; set; }
    }
}
