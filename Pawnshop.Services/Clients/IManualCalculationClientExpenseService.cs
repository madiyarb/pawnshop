using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Clients
{
    public interface IManualCalculationClientExpenseService
    {
        ListModel<ManualCalculationClientExpense> List(ListQuery listQuery);
        ListModel<ManualCalculationClientExpense> List(ListQueryModel<ManualCalculationClientExpenseFilter> listQuery);
        ManualCalculationClientExpense Get(int id);
        ManualCalculationClientExpense Save(ManualCalculationClientExpense car);
        void Delete(int id);
        ManualCalculationClientExpense GetByClientId(int clientId);
    }
}
