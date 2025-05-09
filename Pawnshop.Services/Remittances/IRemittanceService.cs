using Pawnshop.Data.Models.CashOrders;
using System.Threading.Tasks;

namespace Pawnshop.Services.Remittances
{
    public interface IRemittanceService
    {
        Task<Remittance> GetAsync(int remittanceId);
        Remittance Update(int id, int branchIdTo, decimal sum, string note, int authorId, int? branchId = null);
        Remittance Register(int branchIdFrom, int branchIdTo, decimal sum, string note, int authorId);
        Task<Remittance> RegisterAsync(int branchIdFrom, int branchIdTo, decimal sum, string note, int authorId);
        Remittance Accept(int id, int authorId, int? branchId = null);
        Task CancelAcceptAsync(int remittanceId, int authorId, int? branchId = null);
        void Delete(int id, int? branchId = null);
    }
}
