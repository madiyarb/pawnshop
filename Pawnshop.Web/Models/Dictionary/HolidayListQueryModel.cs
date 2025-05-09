using System;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Models.Dictionary
{
    public class HolidayListQueryModel
    {
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}