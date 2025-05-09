using Pawnshop.Data.Models.Insurances;
using System;

namespace Pawnshop.Services.Models.Insurance
{
    public class InsuranceRequestDataModel
    {
        public InsurancePoliceRequest InsurancePoliceRequest { get; set; }
        public decimal Cost { get; set; }
        public DateTime? AdditionDate { get; set; }
        public int? SettingId { get; set; }
    }
}