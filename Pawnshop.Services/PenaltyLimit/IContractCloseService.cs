using System;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.PenaltyLimit
{
    public interface IContractCloseService
    {
        public ContractAction Exec(Contract contract, DateTime date, int authorId, ContractAction childAction = null, OrderStatus? orderStatus = null);

        public ContractAction CloseContractByCreditLine(Contract contract, DateTime date, int authorId,
            ContractAction childAction = null, OrderStatus? orderStatus = null);
    }
}