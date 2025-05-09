using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ClientExternalValidationData
{
    [Table("ClientExternalValidationData")]
    public sealed class ClientExternalValidationData
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        public int ClientId { get; set; }

        public DateTime ValidationDate { get; set; }

        public bool DebtorRegistryValidation { get; set; }

        public bool SUSNValidation { get; set; }

        public bool BankruptValidation { get; set; }

        public ClientExternalValidationData()
        {
            
        }

        public ClientExternalValidationData(Guid id, int clientId, DateTime validationDate)
        {
            Id = id;
            ClientId = clientId;
            ValidationDate = validationDate;
        }

        public void ToSuccess()
        {
            DebtorRegistryValidation = true;
            SUSNValidation = true;
            BankruptValidation = true;
        }
    }
}
