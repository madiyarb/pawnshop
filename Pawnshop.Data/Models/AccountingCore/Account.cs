using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class Account : Pawnshop.AccountingCore.Models.Account, IEntity
    {
        public Account()
        {

        }

        public Account(Pawnshop.AccountingCore.Models.Account model)
        {
            Id = model.Id;
            Name = model.Name;
            NameAlt = model.NameAlt;
            Code = model.Code;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            DeleteDate = model.DeleteDate;
            CurrencyId = model.CurrencyId;
            ContractId = model.ContractId;
            ClientId = model.ClientId;
            AccountNumber = model.AccountNumber;
            OpenDate = model.OpenDate;
            CloseDate = model.CloseDate;
            AccountPlanId = model.AccountPlanId;
            AccountSettingId = model.AccountSettingId;
            Balance = model.Balance;
            BalanceNC = model.BalanceNC;
            LastMoveDate = model.LastMoveDate;
            BranchId = model.BranchId;
            IsOutmoded = model.IsOutmoded;
            RedBalanceAllowed = model.RedBalanceAllowed;
            AccountSettingCode = model.AccountSettingCode;
        }
    }
}
