using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Clients
{
    public interface IClientBlackListService
    {
        List<ClientsBlackListDto> GetAddedList(int clientId);
        List<ClientsBlackList> SaveBlackList(int clientId, List<ClientsBlackListDto> blacklList);
        public List<ClientsBlackListDisplayedDto> GetDisplayedList(int clientId);
        bool CheckClientIsInBlackList(int clientId, ContractActionType actionType, int? contractId = null);
        bool IsClientInBlackList(int clientId);
        public ClientsBlackList Find(object query);
        public void Save(ClientsBlackList entity);
        List<ClientsBlackList> GetClientsBlackListsByClientId(int clientId);
        public Task<List<ClientsBlackList>> GetClientsBlackListsByClientIdAsync(int clientId);
        Task InsertIntoBlackListAsync(IEnumerable<string> iin);
        Task InsertIntoBlackListAsync(int clientId);
    }
}
