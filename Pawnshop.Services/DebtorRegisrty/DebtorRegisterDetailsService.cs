using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Services.DebtorRegisrty.Dtos;
using Pawnshop.Services.DebtorRegisrty.HttpService;
using Pawnshop.Services.DebtorRegisrty.Interfaces;

namespace Pawnshop.Services.DebtorRegisrty
{
    public class DebtorRegisterDetailsService : IDebtorRegisterDetailsService
    {
        private readonly IDebtorRegisterHttpService _debtorRegisterHttpService;

        public DebtorRegisterDetailsService(IDebtorRegisterHttpService debtorRegisterHttpService)
        {
            _debtorRegisterHttpService = debtorRegisterHttpService;
        }
        
        public async Task<List<DebtorDetailsResponseDto>> Details(string iin)
        {
            return await _debtorRegisterHttpService.Details(iin);
        }
    }
}