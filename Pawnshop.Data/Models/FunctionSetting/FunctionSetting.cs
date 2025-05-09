using System;

namespace Pawnshop.Data.Models.FunctionSetting
{
    public class FunctionSetting
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int? NumericValue { get; set; }
        public decimal? MoneyValue { get; set; }
        public string StringValue { get; set; }
        public bool? BooleanValue { get; set; }
    }
}
