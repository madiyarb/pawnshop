using System;

namespace Pawnshop.Data.Models.LegalCollection
{
    public class GetRangeHolidaysRequest
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateUntil { get; set; }
    }
}