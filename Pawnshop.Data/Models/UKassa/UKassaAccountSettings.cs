using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.UKassa
{
    public class UKassaAccountSettings : IEntity
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int KassaId { get; set; }
        public int SectionId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int AuthorId { get; set; }

        public virtual Account Account { get; set; }
    }
}
