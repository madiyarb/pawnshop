using Pawnshop.Data.Models.KFM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.KFM
{
    public interface IKFMService
    {
        Task<bool> FindByClientIdAsync(int clientId);
        Task<bool> FindByIdentityNumberAsync(string identityNumber);
        Task<IEnumerable<KFMPerson>> FindListAsync(object query);
        Task<List<KFMPerson>> GetListAsync();
    }
}
