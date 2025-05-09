using System.Collections.Generic;

namespace Pawnshop.Services.Estimation.Request
{
    public sealed class EstimationRequest
    {
        public string App_id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Middle_name { get; set; }
        public string Birthday { get; set; }
        public string Doc_type { get; set; }
        public List<Gallery> Gallery { get; set; }
        public string Individual_id_number { get; set; }
        public string License_number { get; set; }
        public string License_date_of_issue { get; set; }
        public string License_date_of_end { get; set; }
        public string Place_of_birth { get; set; }
        public string License_issuer { get; set; }
        public string Registration_number { get; set; }
        public string Body_number { get; set; }
        public string License_plate { get; set; }
        public string Registration_issue_date { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Model_id { get; set; }
        public string Color { get; set; }
        public string Holder_name { get; set; }
        public string Prod_year { get; set; }
        public List<LicenseGallery> License_gallery { get; set; }
        public string Indebtedness { get; set; }
        public string Amount { get; set; }
        public string Interest_rate { get; set; }
        public string Type { get; set; }
        public string Garantee { get; set; }
        public string Notes { get; set; }
        public string Is_refinance { get; set; }
        public string Credit_type { get; set; }
        public string Parent_contract_id { get; set; }
        public List<CarGallery> Car_gallery { get; set; }
        public RefinanceData Date { get; set; }
    }
}