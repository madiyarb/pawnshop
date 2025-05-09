using Dapper.Contrib.Extensions;
using System;

namespace Pawnshop.Data.Models.ClientAdditionalContactLogItems
{
    [Table("ClientAdditionalContactLogItems")]
    public sealed class ClientAdditionalContactLogItem : ClientAdditionalContactLogData
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }

        public ClientAdditionalContactLogItem(ClientAdditionalContactLogData data, int? userId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UserId = userId;
            ClientAdditionalContactId = data.ClientAdditionalContactId;
            ClientId = data.ClientId;
            PhoneNumber = data.PhoneNumber;
            ContactOwnerTypeId = data.ContactOwnerTypeId;
            ContactOwnerFullname = data.ContactOwnerFullname;
            DeleteDate = data.DeleteDate;
        }


    }
}
