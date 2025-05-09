using System;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Services.PenaltyLimit
{
    public interface IPenaltyLimitAccrualService
    {
        public void Execute(Contract contract, DateTime date, int authorId);
        public void Execute(Contract contract, Contract parentContract, DateTime date, int authorId);
        void ManualPenaltyLimitAccrual(Contract contract, DateTime date, int authorId);
    }
}