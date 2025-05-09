using Pawnshop.Data.Models.Insurances;
using System;

namespace Pawnshop.Web.Models.Insurance
{
    public class InsuranceReviseListQueryModel
    {
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int InsuranceCompanyId { get; set; }
    }
}
