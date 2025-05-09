using Pawnshop.AccountingCore.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

//Удалить файл класса после успешного маппинга
namespace Pawnshop.Data.Models.Auction.Mapping
{
    public class AuctionCarDetails
    {
        public string AuthorName { get; set; }
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string Branch { get; set; }
        public int ClientId { get; set; }
        public string FullName { get; set; }
        public string IIN { get; set; }
        public int CarId { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public int ReleaseYear { get; set; }
        public string Color { get; set; }
        public string BodyNumber { get; set; }
        public string TransportNumber { get; set; }
        public string AuctionContractNumber {  get; set; }
        public DateTimeOffset AuctionDate { get; set; }
        public DateTimeOffset AuctionContractDate { get; set; }
    }
}
