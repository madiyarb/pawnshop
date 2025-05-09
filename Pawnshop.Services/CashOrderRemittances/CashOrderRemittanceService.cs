using System.Threading.Tasks;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;

namespace Pawnshop.Services.CashOrderRemittances
{
    public class CashOrderRemittanceService : ICashOrderRemittanceService
    {
        private readonly ICashOrderRemittanceRepository _orderRemittanceRepository;

        public CashOrderRemittanceService(ICashOrderRemittanceRepository orderRemittanceRepository)
        {
            _orderRemittanceRepository = orderRemittanceRepository;
        }

        public async Task Create(CashOrderRemittance cashOrderRemittance)
        {
            await _orderRemittanceRepository.Insert(cashOrderRemittance);
        }

        public async Task<CashOrderRemittance> GetById(int id)
        {
            return await _orderRemittanceRepository.GetById(id);
        }

        public async Task<CashOrderRemittance> GetByCashOrderId(int cashOrderId)
        {
            return await _orderRemittanceRepository.GetByCashOrderId(cashOrderId);
        }

        public async Task<CashOrderRemittance> GetByRemittanceId(int remittanceId)
        {
            return await _orderRemittanceRepository.GetByRemittanceId(remittanceId);
        }
    }
}