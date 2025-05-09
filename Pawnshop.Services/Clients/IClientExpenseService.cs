using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Models.Clients;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Pawnshop.Services.Clients
{
    public interface IClientExpenseService
    {
        ClientExpense Get(int clientId);
        ClientExpense Save(int clientId, ClientExpenseDto clientExpense);
        bool IsClientExpenseFilled(int clientId);
        IDbTransaction BeginClientExpenseTransaction();
    }
}
