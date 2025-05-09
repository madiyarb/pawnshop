using Pawnshop.AccountingCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Sellings
{
    public class SellingDiscountOrder
    {
        public int DiscOrder { set; get; }
        public AmountType AmountType { set; get; }
        public decimal Outstanding { set; get; }
    }
}
