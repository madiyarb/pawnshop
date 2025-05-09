using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.LoanSettings
{
    public class LoanSettingProductTypeLTV : IEntity
    {
        public int Id { get; set; }
        public CollateralType CollateralType { get; set; }
        public int SubCollateralTypeId { get; set; }
        public DomainValue SubCollateralType { get; set; }
        public int LoanSettingId { get; set; }
        public LoanSettingProductTypeLTV? LoanSetting { get; set; }
        public decimal LTV { get; set; }
        public DateTime BeginDate { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set;}
    }
}
