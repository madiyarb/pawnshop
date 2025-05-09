using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Services.Estimation.v2.Request
{
    public class PledgeCreationRequest
    {
        public string Brand { get; set; }
        public string Registration_Number { get; set; }
        public string Vin_Code { get; set; }
        public string License_Plate { get; set; }
        public string Registration_Issue_Date { get; set; }
        public string Model { get; set; }
        public int Model_Id { get; set; }
        public string Color { get; set; }
        public string Holder_Name { get; set; }
        public int Prod_Year { get; set; }
        public IEnumerable<GalleryItem> Gallery { get; set; }


        public PledgeCreationRequest() { }

        public PledgeCreationRequest(
            string brand,
            string registrationNumber,
            string vinCode,
            string licensePlate,
            string registrationIssueDate,
            string model,
            int modelId,
            string color,
            string holderName,
            int prodYear,
            IEnumerable<int> galleryItemIds)
        {
            Brand = brand;
            Registration_Number = registrationNumber;
            Vin_Code = vinCode;
            License_Plate = licensePlate;
            Registration_Issue_Date = registrationIssueDate;
            Model = model;
            Model_Id = modelId;
            Color = color;
            Holder_Name = holderName;
            Prod_Year = prodYear;
            Gallery = galleryItemIds?.ToList().Select(x => new GalleryItem { Id = x });
        }
    }
}
