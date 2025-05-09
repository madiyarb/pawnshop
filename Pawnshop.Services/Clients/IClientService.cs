using System.Threading.Tasks;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.Dtos.Client;
using Pawnshop.Data.Models.Clients;
using System.Threading.Tasks;

namespace Pawnshop.Services.Clients
{
    public interface IClientService
    {
        void CheckClientExists(int clientId);
        Client Get(int clientId);
        Client GetOnlyClient(int clientId);
        AuctionClientDto GetByIin(string clientIin);
        Task<Client> GetOnlyClientAsync(int clientId);
        ClientProfile GetClientProfile(int clientId);
        void Save(Client entity);
        Client GetBankByName(string bankName);
        int GetClientAge(Client client);
        Client SetASPStatus(int clientId);
        Task<int> GetClientIdAsync(string iin);
        Task SaveAsync(Client entity);
        Task<int> CreateSimpleClientAsync(CreateSimpleClientCommand command);
    }
}
