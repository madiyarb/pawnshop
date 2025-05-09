using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.UKassa;
using Pawnshop.Services.Models.UKassa;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.UKassa
{
    public interface IUKassaService
    {
        void CreateCheckRequest(CashOrder order);
        void GenerateCheck(int authorId, int accountId, int cashOrderId, int? clientId, int operationType, int paymentType, string itemName, int quantity, decimal itemPrice);
        void CashOperation(int authorId, int accountId, int cashOrderId, int operationType, decimal amount);
        void ResendRequest(int requestId);
        void FinishRequests(List<int> orders);
    }
}
