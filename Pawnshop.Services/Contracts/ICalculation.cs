using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Contracts
{
    public interface ICalculation
    {
        void Init(Contract contract, DateTime? date = null, ContractActionType? actionType = null, bool balanceAccountsOnly = false, decimal refinance = 0);
        /// <summary>
        /// Сумма для отображения в онлайн системах
        /// </summary>
        public decimal DisplayAmount { get; set; }

        public string Reason { get; set; }
    }
}
