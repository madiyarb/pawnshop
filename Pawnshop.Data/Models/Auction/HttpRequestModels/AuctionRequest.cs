using System;
using Pawnshop.Data.Models.Auction.Dtos.Client;

namespace Pawnshop.Data.Models.Auction.HttpRequestModels
{
    public class AuctionRequest
    {
        /// <summary>
        /// номер аукциона
        /// </summary>
        public string AuctionNumber { get; set; }

        /// <summary>
        /// Договор купли продажи
        /// </summary>
        public string AuctionContractNumber { get; set; }

        /// <summary>
        /// Дата договора купли продажи
        /// </summary>
        public DateTimeOffset AuctionContractDate { get; set; }

        /// <summary>
        /// Дата аукциона
        /// </summary>
        public DateTimeOffset AuctionDate { get; set; }

        /// <summary>
        /// Сумма продажи
        /// </summary>
        public decimal AuctionCost { get; set; }
    
        public CreateAuctionClientDto Client { get; set; }

        /// <summary>
        /// Заметка
        /// </summary>
        public string? Note { get; set; }
    }
}