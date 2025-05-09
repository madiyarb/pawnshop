using System;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Web.Models.Insurance
{
    public class InsuranceListQueryModel
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public InsuranceStatus? Status { get; set; }

        public int OwnerId { get; set; }
    }
}
