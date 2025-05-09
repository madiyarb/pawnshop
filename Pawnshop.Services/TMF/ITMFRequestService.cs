using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.TMF;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.TMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.TMF
{
    public interface ITMFRequestService
    {
        TmfClientContractModel GetContractsByIdentityNumber(string identityNumber);
        List<CashOrder> SavePayment(TMFPaymentModel paymentModel, int branchId);
        void PrepareAndSendRequest(CashOrder cashOrder);
        ListModel<TMFPayment> GetTmfPaymentList(ListQueryModel<TmfPaymentFilter> listQuery);
    }
}
