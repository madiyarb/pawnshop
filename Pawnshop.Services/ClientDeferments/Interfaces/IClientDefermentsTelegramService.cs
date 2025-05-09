using Pawnshop.Data.Models.ClientDeferments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.ClientDeferments.Interfaces
{
    public interface IClientDefermentsTelegramService
    {
        Task SendMessageToTelegramNewRecruit(ClientDeferment deferment, string iin, string fullName, string contractNumber);

        Task SendMessageToTelegramUpdateContract(
            ClientDeferment deferment,
            string iin, string fullName,
            string contractNumber,
            bool isMilitary,
            string defermentTypeName);

        Task SendMessageToTelegramUpdateRecruit(ClientDeferment deferment, string iin, string fullName, string contractNumber);
    }
}
