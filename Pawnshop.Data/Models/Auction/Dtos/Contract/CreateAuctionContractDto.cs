namespace Pawnshop.Data.Models.Auction.Dtos.Contract
{
    public class CreateAuctionContractDto
    {
        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        public int ExternalContractId { get; set; }
        public string ContractNumber { get; set; }
        public string Branch { get; set; }
    }
}