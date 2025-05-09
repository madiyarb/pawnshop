namespace Pawnshop.Web.Models.ClientAddresses
{
    public sealed class ClientAddressBinding
    {
        public int AddressTypeId { get; set; }
        public int? CountryId { get; set; }
        public int? ATEId { get; set; }
        public int? GeonimId { get; set; }
        public string BuildingNumber { get; set; }
        public string RoomNumber { get; set; }
        public bool IsActual { get; set; }
        public string Note { get; set; }
    }
}
