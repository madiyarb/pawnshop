using Pawnshop.Data.Models.Contracts.LoanFinancePlans;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Services.Models.List;
using static Pawnshop.Services.Contracts.LoanFinancePlans.LoanFinancePlanSerivce;

namespace Pawnshop.Services.Contracts.LoanFinancePlans
{
    public interface ILoanFinancePlanSerivce: IBaseService<LoanFinancePlan>
    {
        List<LoanFinancePlan> GetList(int contractId);
        List<LoanFinancePlan> SaveFinancePlans(int contractId, List<LoanFinancePlan> loanFinancePlansRequest);
        decimal GetAvailBalance(int clientId, AvailBalanceRequest availBalanceRequest);
    }
}
