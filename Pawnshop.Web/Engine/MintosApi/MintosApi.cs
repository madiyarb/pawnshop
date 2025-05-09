using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Mintos.AnswerModels;
using System.Net.Http;
using Pawnshop.Data.Models.Audit;
using Newtonsoft.Json;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using System.Dynamic;
using System.Text;
using Pawnshop.Web.Models.Mintos;
using RestSharp;

namespace Pawnshop.Web.Engine.MintosApi
{
    public class MintosApi
    {
        private readonly string _mintosUrl;
        private readonly EventLog _eventLog;
        private readonly EnviromentAccessOptions _options;

        public MintosApi(EventLog eventLog, IOptions<EnviromentAccessOptions> options)
        {
            _eventLog = eventLog;
            _options = options.Value;
            _mintosUrl = options.Value.MintosUrl;
        }

        /// <summary>
        /// Запрос GET по одному или по всем договорам
        /// </summary>
        /// <param name="requestUrl">Название раздела</param>
        /// <param name="apiKey">Ключ доступа</param>
        /// <param name="entityId">Идентификатор(если NULL - не подставит)</param>
        /// <returns></returns>
        public AnswerContractModel TryGetOneContract(string requestUrl, string apiKey, int id, int? entityId = null)
        {

            requestUrl = String.Concat("/", requestUrl, id > 0 ? $"/{id}" : "/");
            var client = new RestClient(GetUrl(apiKey, requestUrl))
            {
                Timeout = -1
            };
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            _eventLog.Log(EventCode.MintosContractStatusCheck,
                response.IsSuccessful ? EventStatus.Success : EventStatus.Failed,
                EntityType.MintosContract,
                entityId,
                requestUrl,
                response.Content,
                GetUrl(apiKey, requestUrl));

            if (response.IsSuccessful)
            {
                return JsonConvert.DeserializeObject<AnswerContractModel>(response.Content);
            }
            else throw response.ErrorException;
        }


        /// <summary>
        /// Запрос GET по одному или по всем договорам
        /// </summary>
        /// <param name="requestUrl">Название раздела</param>
        /// <param name="apiKey">Ключ доступа</param>
        /// <param name="entityId">Идентификатор(если NULL - не подставит)</param>
        /// <returns></returns>
        public MintosContractsWithStatuses TryGetAll(string requestUrl, string apiKey)
        {
            var client = new RestClient(GetUrl(apiKey, string.Concat("/", requestUrl)))
            {
                Timeout = -1
            };
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            _eventLog.Log(EventCode.MintosContractStatusCheck,
                response.IsSuccessful ? EventStatus.Success : EventStatus.Failed,
                EntityType.None,
                requestData: requestUrl,
                responseData: response.Content,
                uri: GetUrl(apiKey, requestUrl));

            if (response.IsSuccessful)
            {

                return JsonConvert.DeserializeObject<MintosContractsWithStatuses>(response.Content);
            }

            throw response.ErrorException;
        }

        /// <summary>
        /// Запрос POST выгрузка договора 
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="apiKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string TryPost(string requestUrl, string apiKey, object data, EventCode eventCode = EventCode.MintosContractUpload, EntityType entityType = EntityType.None, int? entityId = null)
        {

            dynamic param = new ExpandoObject();
            param.data = data;
            var json = JsonConvert.SerializeObject(param);

            var client = new RestClient(GetUrl(apiKey, string.Concat("/", requestUrl)))
            {
                Timeout = -1
            };
            var request = new RestRequest(Method.POST);
            //request.AddJsonBody(json);
            request.AddHeader("Accept", "application/json");
            request.Parameters.Clear();
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            _eventLog.Log(eventCode,
                response.IsSuccessful ? EventStatus.Success : EventStatus.Failed,
                entityType,
                entityId,
                json,
                response.Content,
                GetUrl(apiKey, string.Concat("/", requestUrl)));

            if (response.IsSuccessful)
            {
                return response.Content;
            }
            else throw response.ErrorException;
        }


        private string GetUrl(string apiKey, string requestUrl)
        {
            return $@"{_mintosUrl}{apiKey}{requestUrl}";
        }
    }
}
