using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services.Models.TasOnline;
using RestSharp;
using RestSharp.Authenticators;

namespace Pawnshop.Services.TasOnline
{
    public class TasOnlinePaymentApi : ITasOnlinePaymentApi
    {
        private readonly IEventLog _eventLog;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;

        public TasOnlinePaymentApi(
            IEventLog eventLog,
            OuterServiceSettingRepository outerServiceSettingRepository)
        {
            _eventLog = eventLog;
            _outerServiceSettingRepository = outerServiceSettingRepository;
        }

        public TasOnlineResponse Send(string url, object parameters)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new PawnshopApplicationException("Не указан url");

            if (parameters is null)
                throw new PawnshopApplicationException("Параметры передаваемые в апи тас онлайн пусты");

            IRestResponse response = new RestResponse();

            var model = new TasOnlineResponse();

            var configuration = _outerServiceSettingRepository.Find(new {Code = Constants.TAS_ONLINE_INTEGRATION_SETTINGS_CODE });

            if (configuration is null)
                throw new PawnshopApplicationException("Настройки для отправки в ТасОнлайн не заполнены в конфигурации организации");

            var fullUrl = string.Concat(configuration.URL, url);
            var client = new RestClient(fullUrl)
            {
                Timeout = -1
            };

            client.Authenticator = new HttpBasicAuthenticator(configuration.Login, configuration.Password);

            var request = new RestRequest(Method.GET);

            foreach (var property in parameters.GetType().GetProperties())
                request.AddParameter(property.Name, property.GetValue(parameters));

            var uri = client.BuildUri(request).AbsoluteUri;
            model.Url = uri;

            try
            {
                response = client.Execute(request);
            }
            catch (Exception exception)
            {
                _eventLog.Log(EventCode.TasOnlineRequestSending, EventStatus.Failed, requestData: uri, responseData: exception.Message, uri: fullUrl, entityType: null);
            }

            model.Content = response.IsSuccessful ? response.Content : null;
            model.ErrorMessage = response.ErrorMessage;

            return model;
        }
    }
}