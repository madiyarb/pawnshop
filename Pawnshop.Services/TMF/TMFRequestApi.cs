using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services.Models.TasOnline;
using RestSharp.Authenticators;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Services.Models.TMF;
using Newtonsoft.Json;
using System.Reflection;
using Pawnshop.Data.Models.TMF;
using RestSharp.Serialization.Json;
using System.Linq;

namespace Pawnshop.Services.TMF
{
    public class TMFRequestApi : ITMFRequestApi
    {
        private readonly IEventLog _eventLog;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;

        public TMFRequestApi(
            IEventLog eventLog,
            OuterServiceSettingRepository outerServiceSettingRepository)
        {
            _eventLog = eventLog;
            _outerServiceSettingRepository = outerServiceSettingRepository;
        }

        public TMFBaseResponse Send(TMFBaseRequest tmfRequest, object parameters)
        {

            IRestResponse response = new RestResponse();

            var model = new TMFBaseResponse();

            var configuration = _outerServiceSettingRepository.Find(new { Code = Constants.TMF_INTEGRATION_SETTINGS_CODE });

            if (configuration is null)
                throw new PawnshopApplicationException("Настройки для отправки в TMF не заполнены в конфигурации организации");


            var client = new RestClient(configuration.URL);
            var request = new RestRequest(configuration.ControllerURL, Method.POST);
            request.AddHeader(configuration.Login, configuration.Password);
            request.AddJsonBody(tmfRequest);

            try
            {
                response = client.Execute(request);
                //log for successfull response from TMF, in case it will crash, we will have their response logged
                _eventLog.Log(EventCode.TmfRequestSending, EventStatus.Success, requestData: JsonConvert.SerializeObject(tmfRequest), responseData: JsonConvert.SerializeObject(response.Content), uri: tmfRequest.MethodName, entityType: null);
                //remove first character from content because for some reason response.Content has unknown character at first position - works when removed
                var contentString = response.Content.Remove(0, 1);
                var testingModel = JsonConvert.DeserializeObject<TEMPTMFBaseResponse>(contentString);
                model = JsonConvert.DeserializeObject<TMFBaseResponse>(contentString);
                model.Result = testingModel.Result.FirstOrDefault();
            }
            catch (Exception exception)
            {
                _eventLog.Log(EventCode.TmfRequestSending, EventStatus.Failed, requestData: JsonConvert.SerializeObject(tmfRequest), responseData: exception.Message, uri: tmfRequest.MethodName, entityType: null);
            }

            return model;
        }

    }
}
