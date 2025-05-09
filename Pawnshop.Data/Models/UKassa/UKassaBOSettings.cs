using Pawnshop.Core;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.UKassa
{
    public class UKassaBOSettings : IEntity
    {
        public int Id { get; set; }
        public int BusinessOperationSettingId { get; set; }
        public int? NomenclatureId { get; set; }
        public int? CheckOperationType { get; set; }
        public int? CheckStornoOperationType { get; set; }
        public int? CashOperationType { get; set; }
        public int? CashStornoOperationType { get; set; }
        public int PaymentType { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public DateTime DeleteDate { get; set; }
        public bool IsDebit { get; set; }

        public virtual BusinessOperation BusinessOperation { get; set; }
        public virtual DomainValue Nomenclature { get; set; }
    }
}
