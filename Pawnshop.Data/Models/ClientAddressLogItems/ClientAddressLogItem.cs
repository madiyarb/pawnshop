using Dapper.Contrib.Extensions;
using System;

namespace Pawnshop.Data.Models.ClientAddressLogItems
{
    [Table("ClientAddressLogItems")]
    public sealed class ClientAddressLogItem : ClientAddressLogData
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }

        public ClientAddressLogItem()
        {
            
        }

        public ClientAddressLogItem(ClientAddressLogData logData, int? userId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UserId = userId;
            ClientId = logData.ClientId;
            AddressId = logData.AddressId;
            AddressTypeId = logData.AddressTypeId;
            FullPathKaz = logData.FullPathKaz;
            FullPathRus = logData.FullPathRus;
            CountryId = logData.CountryId;
            DeleteDate = logData.DeleteDate;

        }
    }
}
