using Pawnshop.Data.Models.Auction.Dtos.Client;
using Pawnshop.Data.Models.Auction.Dtos.Contract;

namespace Pawnshop.Data.Models.Auction.Dtos.Car
{
    public class CreateAuctionCarDto
    {
        /// <summary>
        /// Id авто предоставленный внешним микросервисом.
        /// </summary>
        public int ExternalCarId { get; set; }
        public string Color { get; set; }
        public string VinCode { get; set; }
    
        /// <summary>
        /// Номер авто при оформлении микро кредита
        /// </summary>
        public string TransportNumber { get; set; }
    
        /// <summary>
        /// Марка авто
        /// <example> Kia </example>
        /// </summary>
        public string Brand { get; set; }
    
        /// <summary>
        /// Модель авто
        /// <example> Rio </example>
        /// </summary>
        public string Model { get; set; }
    
        /// <summary>
        /// Год выпуска
        /// </summary>
        public int ReleaseYear { get; set; }
    
        /// <summary>
        /// Данные договора при оформлении микро кредита
        /// </summary>
        public CreateAuctionContractDto Contract { get; set; }
    
        /// <summary>
        /// Данные клиента при микро займе
        /// </summary>
        public CreateAuctionClientDto Client { get; set; }
    }
}