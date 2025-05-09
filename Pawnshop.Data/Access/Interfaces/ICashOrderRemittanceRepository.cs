using System.Data;
using System.Threading.Tasks;
using Pawnshop.Data.Models;

namespace Pawnshop.Data.Access.Interfaces
{
    public interface ICashOrderRemittanceRepository
    {
        IDbTransaction BeginTransaction();
        Task Insert(CashOrderRemittance cashOrderRemittance);
        Task<CashOrderRemittance> GetById(int id);
        Task<CashOrderRemittance> GetByCashOrderId(int cashOrderId);
        Task<CashOrderRemittance> GetByRemittanceId(int remittanceId);
    }
}