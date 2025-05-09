using Dapper.Contrib.Extensions;
using System;
using System.Reflection.Metadata;

namespace Pawnshop.Data.Models.ClientDocumentLogItems
{
    [Table("ClientDocumentLogItems")]
    public sealed class ClientDocumentLogItem : ClientDocumentLogData
    {

        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }

        public ClientDocumentLogItem(ClientDocumentLogData data, int? userId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UserId = userId;
            DocumentId = data.DocumentId;
            ClientId = data.ClientId;
            Number = data.Number;
            Date = data.Date;
            DateExpire = data.DateExpire;
            ProviderId = data.ProviderId;
            BirthPlace = data.BirthPlace;
            DeleteDate = data.DeleteDate;
        }
    }
}
