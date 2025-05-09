namespace Pawnshop.Data.Models.Auction.Dtos.Client
{
    public class AuctionClientDto
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string FullName { get; set; }
        public int LegalFormId { get; set; }
        public string? IdentityNumber { get; set; }
    }
}