using System.Collections.Generic;

namespace Pawnshop.Data.Models.MobileApp
{
    public class SameContractList
    {
        public List<MobileAppModel> Models { get; set; }
        public List<MobileAppModel> Vehicles { get; set; }
        public List<MobileAppModel> Clients { get; set; }
    }
}