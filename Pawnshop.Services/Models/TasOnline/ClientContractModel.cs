using System.Collections.Generic;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Services.Models.TasOnline
{
    public class ClientContractModel
    {
        public Client Client { get; set; }
        public List<TasOnlineContract> Contracts { get; set; }
    }
}