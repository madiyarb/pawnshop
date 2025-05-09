using System.Threading.Tasks;
using Pawnshop.Data.Models;

namespace Pawnshop.Services.CashOrderRemittances
{
    public interface ICashOrderRemittanceService
    {
        Task Create(CashOrderRemittance cashOrderRemittance);
        Task<CashOrderRemittance> GetById(int id);
        Task<CashOrderRemittance> GetByCashOrderId(int cashOrderId);
        Task<CashOrderRemittance> GetByRemittanceId(int remittanceId);
    }
}