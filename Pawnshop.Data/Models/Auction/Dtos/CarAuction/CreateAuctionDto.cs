using System;
using Pawnshop.Data.Models.Auction.Dtos.Client;

namespace Pawnshop.Data.Models.Auction.Dtos.CarAuction
{
    public class CreateAuctionDto
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
        
        /// <summary>
        /// Сумма списания
        /// </summary>
        public decimal WithdrawCost { get; set; }
    
        public CreateAuctionClientDto Client { get; set; }

        /// <summary>
        /// Заметка
        /// </summary>
        public string? Note { get; set; }
    }
}