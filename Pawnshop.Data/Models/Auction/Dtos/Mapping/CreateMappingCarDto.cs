using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Auction.Dtos.Mapping
{
    public class CreateMappingCarDto
    {
        public int? Iterator { get; set; }
        public string AuthorName { get; set; }

        public CreateCarModel Car { get; set; }
        public CreateAuctionModel? Auction { get; set; }
    }
}
