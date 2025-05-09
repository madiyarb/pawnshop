using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Services.AccountingCore
{
    public interface IAccountBuilderService
    {
        Account OpenForContract(IContract contract, AccountPlan plan, AccountSetting setting);
    }
}