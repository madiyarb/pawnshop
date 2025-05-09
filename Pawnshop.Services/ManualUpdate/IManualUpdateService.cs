using Pawnshop.Data.Models.ManualUpdate;
using System.Threading.Tasks;

namespace Pawnshop.Services.ManualUpdate
{
    public interface IManualUpdateService
    {
        Task<ManualUpdateModel> SendUpdate(ManualUpdateRequest manualUpdate);
    }
}