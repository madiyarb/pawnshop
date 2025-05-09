using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IAccount : IEntity, ICreateLogged, ISoftDelete, IMultiCurrency
    {
        string Name { get; set; }
        int? ContractId { get; set; }
        int? ClientId { get; set; }
        string AccountNumber { get; set; }
        DateTime OpenDate { get; set; }
        DateTime? CloseDate { get; set; }
        int AccountPlanId { get; set; }
        int AccountSettingId { get; set; }
        decimal Balance { get; set; }
        decimal BalanceNC { get; set; }
        DateTime? LastMoveDate { get; set; }
        int BranchId { get; set; }
        bool RedBalanceAllowed { get; set; }
    }
}
