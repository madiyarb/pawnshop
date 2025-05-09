using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Auction.Dtos.Mapping
{
    public class AuctionMappingExpenseApiDto
    {
        public int Iterator { get; set; }
        public int CarId { get; set; }
        public int BranchId { get; set; }
        public int ExpenseTypeId { get; set; }
        public decimal Cost { get; set; }
        public string? Note { get; set; }
        public bool IsFinal {  get; set; }
    }
}
