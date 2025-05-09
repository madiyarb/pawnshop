using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Data.Access;
using Pawnshop.Core;
using Pawnshop.Services.ClientDeferments.Interfaces;
using System;
using Pawnshop.Data.Models.ClientDeferments;
using Pawnshop.Data.Models.Restructuring;
using Pawnshop.Core.Extensions;

namespace Pawnshop.Services.ClientDeferments.Impl
{
    public class ClientDefermentsTelegramService : IClientDefermentsTelegramService
    {
        private readonly OuterServiceSettingRepository _outerServiceSettings;
        private readonly HttpClient _http;

        public ClientDefermentsTelegramService(
            OuterServiceSettingRepository outerServiceSettings,
            HttpClient http)
        {
            _outerServiceSettings = outerServiceSettings;
            _http = http;
        }

        private async Task SendMessage(string title, string message)
        {
            var serviceSetting = _outerServiceSettings.Find(new { Code = Constants.TELEGRAM_RESTRUCTURING_SETTINGS_CODE });
            var parameters = new
            {
                chat_id = serviceSetting.Login,
                text = title + message,
                parse_mode = "Markdown"
            };

            string json = JsonConvert.SerializeObject(parameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(serviceSetting.URL, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendMessageToTelegramNewRecruit(ClientDeferment deferment, string iin, string fullName, string contractNumber)
        {
            var InArmy = deferment.RecruitStatus ? "На службе" : "Не на службе";
            var contractStatus = deferment.Status.ToString();

            await SendMessage("Реструктуризация: Создание отсрочки",
@$"
ИИН: *{iin}*
Имя: {fullName}
Контракт id: *{deferment.ContractId}*
Номер контракта: {contractNumber}
Дата поступления на службу: *{deferment?.StartDate.ToString("dd.MM.yyyy HH:mm:ss")}*
Дата окончания службы: {deferment?.EndDate.ToString("dd.MM.yyyy HH:mm:ss")}
Cтатус клиента: *{InArmy}*
Cтатус контракта: {contractStatus}
");
        }

        public async Task SendMessageToTelegramUpdateContract(
            ClientDeferment deferment,
            string iin, string fullName,
            string contractNumber,
            bool isMilitary,
            string defermentTypeName)
        {
            string clientStatusChanges = $"{defermentTypeName}";

            if (isMilitary)
                clientStatusChanges = deferment.RecruitStatus ? "На службе (Было: Не на службе)" : "Не на службе (Было: На службе)";

            await SendMessage($"Уведомление: Реструктуризация контракта {defermentTypeName}",
$@"
ИИН: *{iin}*
Имя: {fullName}
Контракт ID: *{deferment.ContractId}*
Номер контракта: {contractNumber}
Дата начала: *{deferment?.StartDate.ToString("dd.MM.yyyy HH:mm:ss")}*
Дата окончания: {deferment?.EndDate.ToString("dd.MM.yyyy HH:mm:ss")}
Дата обновления статуса клиента (в системе Fincore): *{deferment?.UpdateDate.ToString("dd.MM.yyyy HH:mm:ss")}*
Статус клиента: {clientStatusChanges}
Статус контракта: *{deferment.Status}*");
        }

        public async Task SendMessageToTelegramUpdateRecruit(ClientDeferment deferment, string iin, string fullName, string contractNumber)
        {
            var clientStatusChanges = deferment.RecruitStatus ? "На службе (Было: Не на службе)" : "Не на службе (Было: На службе)";
            var contractStatus = deferment.Status.ToString();

            await SendMessage("Реструктуризация: Изменен статус службы клиента",
@$"
ИИН: *{iin}*
Имя: {fullName}
Контракт id: *{deferment.ContractId}*
Номер контракта: {contractNumber}
Дата поступления на службу: *{deferment?.StartDate.ToString("dd.MM.yyyy HH:mm:ss")}*
Дата окончания службы: {deferment?.EndDate.ToString("dd.MM.yyyy HH:mm:ss")}
Дата обновления статуса клиента (в системе Fincore): *{deferment?.UpdateDate.ToString("dd.MM.yyyy HH:mm:ss")}*
Cтатус клиента: {clientStatusChanges}
Cтатус контракта: *{contractStatus}*
");
        }
    }
}
