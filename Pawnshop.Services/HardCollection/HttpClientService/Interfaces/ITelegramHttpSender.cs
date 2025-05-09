using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.HardCollection.HttpClientService.Interfaces
{
    public interface ITelegramHttpSender
    {
        Task SendLog(string text);
    }
}
