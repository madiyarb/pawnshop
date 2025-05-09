using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Services.Estimation.v2.Request
{
    public class ClientCreationRequest
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Middle_Name { get; set; }
        public string Birthday { get; set; }
        public string Doc_Type { get; set; }
        public string Iin { get; set; }
        public string License_Number { get; set; }
        public string License_Date_Of_Issue { get; set; }
        public string License_Date_Of_End { get; set; }
        public string Place_Of_Birth { get; set; }
        public string License_Issuer { get; set; }
        public IEnumerable<GalleryItem> Gallery { get; set; }


        public ClientCreationRequest() { }

        public ClientCreationRequest(
            string name,
            string surname,
            string middleName,
            string birthday,
            string docType,
            string iin,
            string licenseNumber,
            string licenseDateOfIssue,
            string licenseDateOfEnd,
            string placeOfBirth,
            string licenseIssuer,
            IEnumerable<int> galleryItemIds)
        {
            Name = name;
            Surname = surname;
            Middle_Name = middleName;
            Birthday = birthday;
            Doc_Type = docType;
            Iin = iin;
            License_Number = licenseNumber;
            License_Date_Of_Issue = licenseDateOfIssue;
            License_Date_Of_End = licenseDateOfEnd;
            Place_Of_Birth = placeOfBirth;
            License_Issuer = licenseIssuer;
            Gallery = galleryItemIds?.ToList().Select(x => new GalleryItem { Id = x });
        }
    }
}
