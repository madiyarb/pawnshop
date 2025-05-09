using Pawnshop.Data.Access;
using Pawnshop.Data.Models.TasLabRecruit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core;
using Newtonsoft.Json;
using Pawnshop.Core.Queries;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services.Clients;
using System.Linq;
using Confluent.Kafka;

namespace Pawnshop.Services.TasLabRecruit
{
    public class TasLabRecruitService : ITasLabRecruitService
    {
        private readonly ISessionContext _sessionContext;
        private readonly TasLabRecruitRequestsRepository _recruitRequestRepository;
        private readonly HttpClient _httpClient;
        private readonly IEventLog _eventLog;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly DomainValueRepository _domainValueRepository;

        public TasLabRecruitService(
            OuterServiceSettingRepository outerServiceSettings,
            ISessionContext sessionContext,
            TasLabRecruitRequestsRepository recruitRequestRepository,
            HttpClient httpClient,
            IEventLog eventLog,
            IClientBlackListService clientBlackListService,
            DomainValueRepository domainValueRepository)
        {
            _sessionContext = sessionContext;
            _recruitRequestRepository = recruitRequestRepository;

            var settings = outerServiceSettings.Find(new { Code = Constants.API_GATEWAY_TASLAB });
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.URL);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(
                        $"{settings.Login}:{settings.Password}")));
            _eventLog = eventLog;
            _clientBlackListService = clientBlackListService;
            _domainValueRepository = domainValueRepository;
        }

        // Получение списка призывников, у кого есть действующие контракты у провайдера
        public async Task<List<Recruit>> GetRecruitsList()
        {
            var request = new RecruitRequest()
            {
                RequestType = RecruitRequestType.GetRecruitList,
                UserId = GetAuthorId(),
                CreateDate = DateTime.Now,
            };
            var response = await RecruitsList(Constants.RECRUIT_ENDPOINT_KEY__FCB_GET_RECRUIT_LIST, Constants.RECRUIT_API__FCB_GET_RECRUIT_LIST);

            request.ErrorMessage = response.Message;
            request.ResponseIndex = response.Index;
            request.CBType = _domainValueRepository.GetByCodeAndDomainCode(Constants.DOMAIN_VALUE_CREDIT_BUREAU_FCB_CODE, Constants.DOMAIN_CREDIT_BUREAUS_CODE).Id;
            request.ResponseJson = JsonConvert.SerializeObject(response.Data);

            _recruitRequestRepository.Insert(request);
            _eventLog.Log(EventCode.GetRecruitList, EventStatus.Success, EntityType.ClientDefermentsForRecruit, entityId: request.Id, requestData: JsonConvert.SerializeObject(request));

            if (!string.IsNullOrEmpty(response.Message))
            {
                _eventLog.Log(EventCode.GetRecruitList, EventStatus.Failed, EntityType.ClientDefermentsForRecruit, entityId: request.Id, requestData: JsonConvert.SerializeObject(request), responseData: response.Message);
                throw new PawnshopApplicationException(response.Message);
            }

            await _clientBlackListService.InsertIntoBlackListAsync(response.Data.Select(x => x.IIN));
            return response.Data;
        }

        // Метод презназначен для получения для получения данных, которые изменились/появились между датами запроса метода list или delta с указанием индекса данных.
        public async Task<List<Recruit>> GetRecruitsDelta()
        {
            int cbType = _domainValueRepository.GetByCodeAndDomainCode(Constants.DOMAIN_VALUE_CREDIT_BUREAU_FCB_CODE, Constants.DOMAIN_CREDIT_BUREAUS_CODE).Id;
            var lastListRequest = _recruitRequestRepository.Find(new ListQuery() { Sort = new Sort("Id", SortDirection.Desc) }, new { RequestType = (int)RecruitRequestType.GetRecruitList, CBType = cbType });
            var request = new RecruitRequest()
            {
                RequestType = RecruitRequestType.GetRecruitDelta,
                UserId = GetAuthorId(),
                RequestIndex = lastListRequest.ResponseIndex,
                CreateDate = DateTime.Now,
            };
            var response = await RecruitsList(Constants.RECRUIT_ENDPOINT_KEY__FCB_GET_RECRUIT_DELTA, Constants.RECRUIT_API__FCB_GET_RECRUIT_DELTA + request.RequestIndex);

            request.ErrorMessage = response.Message;
            request.CBType = cbType;
            request.ResponseJson = JsonConvert.SerializeObject(response.Data);

            _recruitRequestRepository.Insert(request);
            _eventLog.Log(EventCode.GetRecruitDelta, EventStatus.Success, EntityType.ClientDefermentsForRecruit, entityId: request.Id, requestData: JsonConvert.SerializeObject(request));

            if (!string.IsNullOrEmpty(response.Message))
            {
                _eventLog.Log(EventCode.GetRecruitDelta, EventStatus.Failed, EntityType.ClientDefermentsForRecruit, entityId: request.Id, requestData: JsonConvert.SerializeObject(request), responseData: response.Message);
                throw new PawnshopApplicationException(response.Message);
            }

            return response.Data;
        }

        // Метод предназначен для получения статуса по ИИН (ГКБ)
        public async Task<RecruitIINResponse> GetRecruitByIIN(string iin)
        {
            var request = new RecruitRequest()
            {
                RequestType = RecruitRequestType.GetRecruitByIIN,
                UserId = GetAuthorId(),
                RequestIIN = iin,
                CreateDate = DateTime.Now,
            };
            var response = await RecruitByIIN(Constants.RECRUIT_ENDPOINT_KEY__MKB_GET_RECRUIT_BY_IIN, Constants.RECRUIT_API__MKB_GET_RECRUIT_BY_IIN + request.RequestIIN);

            request.ErrorMessage = response.Message;
            request.CBType = _domainValueRepository.GetByCodeAndDomainCode(Constants.DOMAIN_VALUE_CREDIT_BUREAU_MKB_CODE, Constants.DOMAIN_CREDIT_BUREAUS_CODE).Id;

            _recruitRequestRepository.Insert(request);

            if (!string.IsNullOrEmpty(response.Message))
            {
                _eventLog.Log(EventCode.GetRecruitByIIN, EventStatus.Failed, EntityType.ClientDefermentsForRecruit, entityId: request.Id, requestData: JsonConvert.SerializeObject(request), responseData: response.Message);
                throw new PawnshopApplicationException(response.Message);
            }
            _eventLog.Log(EventCode.GetRecruitByIIN, EventStatus.Success, EntityType.ClientDefermentsForRecruit, entityId: request.Id, requestData: JsonConvert.SerializeObject(request));

            return response;
        }

        // Получение списка призывников, у кого есть действующие контракты у провайдера (ГКБ)
        public async Task<List<Recruit>> GetRecruitsListMKB()
        {
            var request = new RecruitRequest()
            {
                RequestType = RecruitRequestType.GetRecruitList,
                UserId = GetAuthorId(),
                CreateDate = DateTime.Now,
            };
            var response = await RecruitsList(Constants.RECRUIT_ENDPOINT_KEY__MKB_GET_RECRUIT_LIST, Constants.RECRUIT_API__MKB_GET_RECRUIT_LIST);

            request.ErrorMessage = response.Message;
            request.ResponseIndex = response.Index;
            request.CBType = _domainValueRepository.GetByCodeAndDomainCode(Constants.DOMAIN_VALUE_CREDIT_BUREAU_MKB_CODE, Constants.DOMAIN_CREDIT_BUREAUS_CODE).Id;
            request.ResponseJson = JsonConvert.SerializeObject(response.Data);

            _recruitRequestRepository.Insert(request);
            _eventLog.Log(EventCode.GetRecruitList, EventStatus.Success, EntityType.ClientDefermentsForRecruit, entityId: request.Id, requestData: JsonConvert.SerializeObject(request));

            if (!string.IsNullOrEmpty(response.Message))
            {
                _eventLog.Log(EventCode.GetRecruitList, EventStatus.Failed, EntityType.ClientDefermentsForRecruit, entityId: request.Id, requestData: JsonConvert.SerializeObject(request));
                throw new PawnshopApplicationException(response.Message);
            }

            await _clientBlackListService.InsertIntoBlackListAsync(response.Data.Select(x => x.IIN));
            return response.Data;
        }

        private async Task<RecruitListResponse> RecruitsList(string endpointKey, string address)
        {
            if (_httpClient.DefaultRequestHeaders.Contains("ENDPOINT-KEY"))
                _httpClient.DefaultRequestHeaders.Remove("ENDPOINT-KEY");

            _httpClient.DefaultRequestHeaders.Add("ENDPOINT-KEY", endpointKey);
            try
            {
                var response = await _httpClient.GetAsync(address);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RecruitListResponse>(responseContent);
            }
            catch (Exception ex)
            {
                return new RecruitListResponse()
                {
                    Message = endpointKey + " " + JsonConvert.SerializeObject(ex.Message)
                };
            }
        }

        private async Task<RecruitIINResponse> RecruitByIIN(string address, string endpointKey)
        {
            if (_httpClient.DefaultRequestHeaders.Contains("ENDPOINT-KEY"))
                _httpClient.DefaultRequestHeaders.Remove("ENDPOINT-KEY");

            _httpClient.DefaultRequestHeaders.Add("ENDPOINT-KEY", endpointKey);
            try
            {
                var response = await _httpClient.GetAsync(address);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RecruitIINResponse>(responseContent);
            }
            catch (Exception ex)
            {
                return new RecruitIINResponse()
                {
                    Message = endpointKey + " " + JsonConvert.SerializeObject(ex.Message)
                };
            }
        }

        private int GetAuthorId()
        {
            if (!_sessionContext.IsInitialized)
                return Constants.ADMINISTRATOR_IDENTITY;

            return _sessionContext.UserId;
        }
    }
}
