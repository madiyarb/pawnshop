using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    public partial class Account : IAccount, IDictionary
    {
        public Account()
        {

        }

        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Наименование альтернативное
        /// </summary>
        public string NameAlt { get; set; }

        /// <summary>
        /// Уникальный код
        /// </summary>
        public string Code { get; set; }

        public string AccountSettingCode { get; set; }

        public int AuthorId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? DeleteDate { get; set; }

        public int CurrencyId { get; set; }

        public int? ContractId { get; set; }

        public int? ClientId { get; set; }

        public string AccountNumber { get; set; }

        public DateTime OpenDate { get; set; }

        public DateTime? CloseDate { get; set; }

        public int AccountPlanId { get; set; }

        public int AccountSettingId { get; set; }
        public AccountSetting AccountSetting { get; set; }

        public decimal Balance { get; set; } = 0;

        public decimal BalanceNC { get; set; } = 0;

        public DateTime? LastMoveDate { get; set; }

        public int BranchId { get; set; }

        public bool IsOutmoded { get; set; }

        public bool RedBalanceAllowed { get; set; }
    }
}
