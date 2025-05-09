using Dapper.Contrib.Extensions;
using System;

namespace Pawnshop.Data.Models.ClientLogItems
{
    [Table("ClientLogItems")]
    public sealed class ClientLogItem : ClientLogData
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }

        public ClientLogItem()
        {
            
        }
        public ClientLogItem(ClientLogData data, int? userId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UserId = userId;
            ClientId = data.ClientId;
            Surname = data.Surname;
            Name = data.Name;
            Patronymic = data.Patronymic;
            BirthDay = data.BirthDay;
            IdentityNumber = data.IdentityNumber;
        }
    }
}
