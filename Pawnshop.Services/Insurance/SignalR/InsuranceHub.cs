using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Pawnshop.Services.Insurance.SignalR
{
    public class InsuranceHub: Hub
    {
        public async Task Send(string message)
        {
            await this.Clients.All.SendAsync("Send", message);
        }
    }
}