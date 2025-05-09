using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractStatusHistory : IEntity
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int UserId { get; set; }
        public ContractStatus Status { get; set; }
        public DateTime Date { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
