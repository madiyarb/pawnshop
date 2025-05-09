using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Services.Clients;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionCheckClientDeathService : ILegalCollectionCheckClientDeathService
    {
        private readonly IClientBlackListService _blackListService;

        public LegalCollectionCheckClientDeathService(IClientBlackListService blackListService)
        {
            _blackListService = blackListService;
        }

        public async Task CheckBlackListClientDeath(int ClientId)
        {
            var list = await _blackListService.GetClientsBlackListsByClientIdAsync(ClientId);
            if (list.All(x => x.BlackListReason.Code != Constants.BLACKLIST_REASON_CODE))
                throw new PawnshopApplicationException("Клиент не занесен в Черный список по причине \"Умер\". Сначала требуется занести клиента в Черный список с данной причиной.");
        }
    }
}