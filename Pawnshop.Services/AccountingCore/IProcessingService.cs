using Pawnshop.Data.Models.Contracts;
using System;

namespace Pawnshop.Services.AccountingCore
{
    public interface IProcessingService
    {
        void InitAccruals(Contract contract, DateTime accrualDate);
        DateTime? CalcAccrualDate(DateTime date);
    }
}
