using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Services.DebtorRegisrty.Dtos;

namespace Pawnshop.Services.DebtorRegisrty.Interfaces
{
    public interface IDebtorRegisterDetailsService
    {
        public Task<List<DebtorDetailsResponseDto>> Details(string iin);
    }
}