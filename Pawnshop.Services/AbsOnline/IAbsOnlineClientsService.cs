using Pawnshop.Data.Models.AbsOnline;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.AbsOnline
{
    public interface IAbsOnlineClientsService
    {
        Task<List<AbsOnlineClientPositionView>> GetClientPositionsAsync(string iin);
    }
}
