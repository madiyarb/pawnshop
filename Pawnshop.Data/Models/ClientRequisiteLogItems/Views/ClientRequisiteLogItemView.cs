using System;

namespace Pawnshop.Data.Models.ClientRequisiteLogItems.Views
{
    public sealed class ClientRequisiteLogItemView
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public int RequisiteId { get; set; }
        public int ClientId { get; set; }
        public int RequisiteTypeId { get; set; }
        public string RequisiteTypeName { get; set; }
        public int? BankId { get; set; }
        public string BankName { get; set; }
        public string Value { get; set; }
        public string CardHolderName { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
