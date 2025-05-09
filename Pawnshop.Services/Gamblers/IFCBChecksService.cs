using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Models.Contracts.Kdn;
using System.Threading.Tasks;

namespace Pawnshop.Services.Gamblers
{
    public interface IFCBChecksService
    {
        public Task<FCBChecksResult> GamblerFeatureCheck(int clientId, User author);
    }
}
