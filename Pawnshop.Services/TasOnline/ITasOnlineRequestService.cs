using System.Collections.Generic;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Services.Models.TasOnline;

namespace Pawnshop.Services.TasOnline
{
    public interface ITasOnlineRequestService : IBaseService<TasOnlineRequest>
    {
        ClientContractModel GetContractsByIdentityNumber(string identityNumber);
        List<CashOrder> SavePayment(PaymentModel payment, int branchId);
        TasOnlinePayment CheckPayment(int id);
        TasOnlinePayment RePayment(int id);
    }
}