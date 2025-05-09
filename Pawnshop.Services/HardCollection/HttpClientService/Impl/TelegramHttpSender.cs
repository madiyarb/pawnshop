using Pawnshop.Services.HardCollection.HttpClientService.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pawnshop.Services.HardCollection.HttpClientService.Impl
{
    public class TelegramHttpSender : ITelegramHttpSender
    {
        public TelegramHttpSender() { }

        public async Task SendLog(string text)
        {
            using (var client = new HttpClient())
            {
                await client.GetAsync("https://api.telegram.org/bot6848373031:AAFlYJkP1wtQC-szTsLFaZknK2dYBVw7yTA/sendMessage?chat_id=-1002063051778&text=" + text);
            }
        }
    }
}
