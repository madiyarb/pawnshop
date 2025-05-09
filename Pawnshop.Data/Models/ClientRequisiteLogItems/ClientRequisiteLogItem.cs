using Dapper.Contrib.Extensions;
using System;

namespace Pawnshop.Data.Models.ClientRequisiteLogItems
{
    [Table("ClientRequisiteLogItems")]
    public sealed class ClientRequisiteLogItem : ClientRequisiteLogData
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }

        public ClientRequisiteLogItem()
        {
            
        }

        public ClientRequisiteLogItem(ClientRequisiteLogData data, int? userId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UserId = userId;
            ClientId = data.ClientId;
            RequisiteId = data.RequisiteId;
            RequisiteTypeId = data.RequisiteTypeId;
            BankId = data.BankId;
            Value = data.Value;
            CardHolderName = data.CardHolderName;
            DeleteDate = data.DeleteDate;
        }
    }
}
