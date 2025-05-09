using System;

namespace Pawnshop.Data.Models.ClientDocumentLogItems.Views
{
    public sealed class ClientDocumentLogItemView
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public int ClientId { get; set; }
        public int DocumentId { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateExpire { get; set; }
        public string ProviderName { get; set; }
        public int? ProviderId { get; set; }
        public string BirthPlace { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
