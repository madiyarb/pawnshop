using System.Xml.Serialization;

namespace Pawnshop.Services.KFM.Models
{
    [XmlRoot(ElementName = "person")]
    public class Person
    {
        [XmlElement(ElementName = "num")]
        public int Num { get; set; }

        [XmlElement(ElementName = "lname")]
        public string Lname { get; set; }

        [XmlElement(ElementName = "fname")]
        public string Fname { get; set; }

        [XmlElement(ElementName = "mname")]
        public string Mname { get; set; }

        [XmlElement(ElementName = "birthdate")]
        public string Birthdate { get; set; }

        [XmlElement(ElementName = "iin")]
        public string Iin { get; set; }

        [XmlElement(ElementName = "note")]
        public string Note { get; set; }

        [XmlElement(ElementName = "correction")]
        public string Correction { get; set; }
    }
}
