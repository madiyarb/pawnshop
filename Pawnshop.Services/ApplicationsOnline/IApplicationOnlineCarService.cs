using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.ApplicationsOnline
{
    public interface IApplicationOnlineCarService
    {
        public Task<bool> ActualizeCarInfo(Guid id, int clientId);
    }
}
