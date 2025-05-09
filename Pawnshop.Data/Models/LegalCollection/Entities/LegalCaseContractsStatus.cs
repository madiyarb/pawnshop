using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.LegalCollection.Entities
{
    public class LegalCaseContractsStatus : IEntity
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int LegalCaseId { get; set; }
        public DateTimeOffset? DeleteDate { get; set; }
    }
}