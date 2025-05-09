using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Bankruptcy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Services.Bankruptcy
{
    public class BankruptcyService : IBankruptcyService
    {
        private readonly HttpClient _http;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;
        private readonly ClientRepository _clientRepository;
        private readonly BlackListReasonRepository  _blackListReasonRepository;
        private readonly ClientsBlackListRepository _clientsBlackListRepository;
        private readonly IEventLog _eventLog;
        private enum BankruptcyRequestType : short
        {
            Online = 1, 
            Local = 2,
            Company = 3,
            VATCertificate = 4
        }

        private const string localIndividualShortAPI = "/tgw-request/api/v1/tazalau/by/iin/short/get_bankrupt_info_by_iin_short?iins=";
        private const string onlineIndividualAPI = "/tgw-request/api/v1/tazalau/by/iin/get_bankrupt_info_by_iin?iins=";
        private const string companyAPI = "/tgw-request/api/v1/opendata/bankrupt/approved?bin=";
        private const string vatCertificateAPI = "/tgw-request/api/v1/knp/get/vat_certificate?bin=";
        
        public BankruptcyService(
            HttpClient http,
            OuterServiceSettingRepository outerServiceSettingRepository,
            ClientRepository clientRepository,
            BlackListReasonRepository blackListReasonRepository,
            ClientsBlackListRepository clientsBlackListRepository,
            IEventLog eventLog
            ) 
        {
            _http = http;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _clientRepository = clientRepository;
            _blackListReasonRepository = blackListReasonRepository;
            _clientsBlackListRepository = clientsBlackListRepository;
            _eventLog = eventLog;
        }

        public async Task CheckIndividualClient(string iin)
        {
            Validate(iin);
            // проверка из друх источников
            // 1) BankruptcyRequestType.Local  (из бд шлюза TasLab)
            // 2) BankruptcyRequestType.Online (из гос сервисов)
            if (await ExecuteChecking(iin, BankruptcyRequestType.Local) || await ExecuteChecking(iin, BankruptcyRequestType.Online))
                SetInBlackList(iin);
        }

        public async Task CheckCompany(string bin)
        {
            Validate(bin);
            if (await ExecuteChecking(bin, BankruptcyRequestType.Company))
                SetInBlackList(bin);
        }

        private void Validate(string identityNumber)
        {
            if (identityNumber.Length != 12)
                throw new PawnshopApplicationException("длина ИИН или БИН должна быть 12 символов");
        }

        private string GetApiForChecking(BankruptcyRequestType requestType)
        {
            return requestType switch
            {
                BankruptcyRequestType.Local => localIndividualShortAPI,
                BankruptcyRequestType.Online => onlineIndividualAPI,
                BankruptcyRequestType.Company => companyAPI,
                //BankruptcyRequestType.VATCertificate => vatCertificateAPI,
                _ => throw new NotImplementedException()
            };
        }

        private async Task<bool> ExecuteChecking(string identityNumber, BankruptcyRequestType requestType)
        {
            var clientId = _clientRepository.FindByIdentityNumber(identityNumber).Id;
            var api = GetApiForChecking(requestType);
            var setting = _outerServiceSettingRepository.Find(new { Code = Constants.API_GATEWAY_TASLAB });
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{setting.URL}{api}{identityNumber}");
                request.Headers.Add("accept", "application/json");

                if (requestType == BankruptcyRequestType.Online || requestType == BankruptcyRequestType.Company)
                    request.Headers.Add("ENDPOINT-KEY", "egov_bankrupt_approved_by_bin");

                request.Headers.Add("Authorization", $"Basic {AuthBase64(setting.Login, setting.Password)}");

                // при await, сломается сессия
                var response = _http.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _eventLog.Log(EventCode.CheckClientBankruptcy, EventStatus.Success, EntityType.Client, clientId, request.ToString(), responseContent, setting.URL + api);
                    // конверт в лист так как прилетает ответ прилетает массивом
                    // в лист object так как наличие объекта подтвеждает наличие банкротства
                    var result = JsonConvert.DeserializeObject<List<object>>(responseContent);
                    var bankruptcy = result.FirstOrDefault();
                    return bankruptcy != null;
                }
                else
                {
                    _eventLog.Log(EventCode.CheckClientBankruptcy, EventStatus.Failed, EntityType.Client, clientId, response.StatusCode.ToString(), JsonConvert.SerializeObject(response), setting.URL + api);
                }
            }
            catch (HttpRequestException ex)
            {
                _eventLog.Log(EventCode.CheckClientBankruptcy, EventStatus.Failed, EntityType.Client, clientId, "HttpRequestException", JsonConvert.SerializeObject(ex), setting.URL + api);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _eventLog.Log(EventCode.CheckClientBankruptcy, EventStatus.Failed, EntityType.Client, clientId, "TaskCanceledException", JsonConvert.SerializeObject(ex), setting.URL + api);
                throw;
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.CheckClientBankruptcy, EventStatus.Failed, EntityType.Client, clientId, "Exception", JsonConvert.SerializeObject(ex), setting.URL + api);
                throw;
            }

            return false;
        }

        private void SetInBlackList(string identityNumber)
        {
            var client = _clientRepository.FindByIdentityNumber(identityNumber);
            _clientsBlackListRepository.Insert(new Data.Models.Dictionaries.ClientsBlackList()
            {
                ClientId = client.Id,
                ReasonId = _blackListReasonRepository.Find(new { Name = "Банкрот либо в стадии банкротства" }).Id,
                AddedBy = 1,
                AddReason = "Список банкротства",
                AddedAt = DateTime.Now
            });
        }

        private string AuthBase64(string login, string password)
        {
            byte[] TextBytes = Encoding.UTF8.GetBytes(login +":"+ password);
            return Convert.ToBase64String(TextBytes);
        }
    }
}
