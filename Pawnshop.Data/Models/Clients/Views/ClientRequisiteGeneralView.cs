using System;

namespace Pawnshop.Data.Models.Clients.Views
{
    public sealed class ClientRequisiteGeneralView
    {
        public int Id { get; set; } 
        public int ClientId { get; set; }
        public int RequisiteTypeId { get; set; }
        public string Note { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }
        public bool IsDefault { get; set; }
        public string Value { get; set; }
        public string CardExpiryDate { get; set; }
        public string CardHolderName { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }


    }
}
