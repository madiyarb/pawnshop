namespace Pawnshop.Data.Models.Auction.Dtos.Client
{
    public class CreateAuctionClientDto
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ExternalClientId { get; set; }
        public string IIN { get; set; }
    
        /// <summary>
        /// Имя 
        /// </summary>
        public string FirstName { get; set; }
    
        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; }
    
        /// <summary>
        /// Отчество
        /// </summary>
        public string? MiddleName { get; set; }
    }
}