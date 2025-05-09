using System.Threading.Tasks;

namespace Pawnshop.Services.LegalCollection.Inerfaces
{
    public interface ILegalCollectionCheckClientDeathService
    {
        public Task CheckBlackListClientDeath(int ClientId);
    }
}