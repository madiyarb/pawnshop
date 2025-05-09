using System.Xml.Serialization;

namespace Pawnshop.Services.KFM.Models
{
    [XmlRoot(ElementName = "xml")]
    public class XmlRoot
    {
        [XmlElement(ElementName = "persons")]
        public Persons Persons { get; set; }
    }
}
