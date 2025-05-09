using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Processing;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Services.Contracts
{
    public interface IContractActionPrepaymentService
    {
        ContractAction Exec(int contractId, decimal amount, int payTypeId, int branchId, int authorId, DateTime date, int? employeeId = null,
            ProcessingInfo processingInfo = null, OrderStatus? orderStatus = null);

        ContractAction MovePrepayment(MovePrepayment prepaymentModel, int authorId, Group branch, ContractAction parentAction = null, bool autoApprove = true);
    }
}
