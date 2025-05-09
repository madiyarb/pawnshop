using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Kdn;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.MobileApp;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class RealtyPrintingFormModel
    {
        public Contract Contract { get; set; }
        public User User { get; set; }
    }
}
