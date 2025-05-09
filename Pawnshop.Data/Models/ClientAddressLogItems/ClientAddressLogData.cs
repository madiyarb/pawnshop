using System;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Models.ClientAddressLogItems
{
    public class ClientAddressLogData
    {
        public int ClientId { get; set; }
        public int AddressId { get; set; }
        public int AddressTypeId { get; set; }
        public string FullPathRus { get; set; }
        public string FullPathKaz { get; set; }
        public int? CountryId { get; set; }
        public DateTime? DeleteDate { get; set; }

        public ClientAddressLogData()
        {
            
        }
        public ClientAddressLogData(ClientAddress address)
        {
            ClientId = address.ClientId;
            AddressId = address.Id;
            AddressTypeId = address.AddressTypeId;
            FullPathKaz = address.FullPathKaz;
            FullPathRus = address.FullPathRus;
            CountryId = address.CountryId;
            DeleteDate = address.DeleteDate;
        }
    }
}
