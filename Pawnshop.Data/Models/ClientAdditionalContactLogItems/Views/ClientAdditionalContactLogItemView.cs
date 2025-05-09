using System;

namespace Pawnshop.Data.Models.ClientAdditionalContactLogItems.Views
{
    public sealed class ClientAdditionalContactLogItemView
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public int ClientAdditionalContactId { get; set; }
        public int ClientId { get; set; }
        public string PhoneNumber { get; set; }
        public int ContactOwnerTypeId { get; set; }
        public string ContactOwnerTypeName { get; set; }
        public string ContactOwnerFullname { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
