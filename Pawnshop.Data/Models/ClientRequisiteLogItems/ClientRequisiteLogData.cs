using System;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Models.ClientRequisiteLogItems
{
    public class ClientRequisiteLogData
    {
        public int RequisiteId { get; set; }
        public int ClientId { get; set; }
        public int RequisiteTypeId { get; set; }
        public int? BankId { get; set; }
        public string Value { get; set; }
        public string CardHolderName { get; set; }
        public DateTime? DeleteDate { get; set; }

        public ClientRequisiteLogData()
        {
            
        }

        public ClientRequisiteLogData(ClientRequisite requisite)
        {
            RequisiteId = requisite.Id;
            ClientId = requisite.ClientId;
            RequisiteTypeId = requisite.RequisiteTypeId;
            BankId = requisite.BankId;
            Value = requisite.Value;
            CardHolderName = requisite.CardHolderName;
            DeleteDate = requisite.DeleteDate;
        }
    }
}
