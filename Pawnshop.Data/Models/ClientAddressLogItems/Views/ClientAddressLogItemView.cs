using System;

namespace Pawnshop.Data.Models.ClientAddressLogItems.Views
{
    public sealed class ClientAddressLogItemView
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public int ClientId { get; set; }
        public int AddressId { get; set; }
        public string AddressTypeName { get; set; }
        public int AddressTypeId { get; set; }
        public string FullPathRus { get; set; }
        public string FullPathKaz { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
