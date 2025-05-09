using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.OuterServiceSettings;
using RestSharp;
using System;

namespace Pawnshop.Services.Insurance.InsuranceCompanies.FridomFinance
{
    public class FridomFinanceService : IInsuranceCompanyService
    {
        private readonly OuterServiceCompanyConfig _config;

        public FridomFinanceService(OuterServiceCompanyConfig config)
        {
            _config = config;
        }

        public RegisterInsuranceResponse RegisterPolicy(InsurancePoliceRequest insurancePoliceRequest)
        {
            try
            {
                var requestData = insurancePoliceRequest.RequestData;

                return SendRegisterPolicyRequest(requestData);
            }
            catch (Exception ex)
            {
                throw new PawnshopApplicationException(
                    $@"Не возможно распознать данные с JSON RequestData, проверьте правильность заполнения {ex.Message}");
            }
        }

        public CancelInsuranceResponse CancelPolicy(InsurancePoliceRequest insurancePoliceRequest)
        {
            try
            {
                var requestData = insurancePoliceRequest.RequestData;
                var cancelRequest = new InsurancePoliceCancelRequest
                {
                    PolicyNumber = requestData.ContractNumber,
                    MetaDataItem = requestData.MetaDataItem
                };

                return SendCancelPolicyRequest(cancelRequest);
            }
            catch (Exception ex)
            {
                throw new PawnshopApplicationException(
                    $@"Не возможно распознать данные с JSON RequestData, проверьте правильность заполнения {ex.Message}");
            }
        }

        private RegisterInsuranceResponse SendRegisterPolicyRequest(InsuranceRequestData requestData)
        {
            var result = new RegisterInsuranceResponse();

            var client = new RestClient(GetUrl(_config)) { Timeout = -1 };

            var request = new RestRequest(Method.POST);

            var serializedRequest = JsonConvert.SerializeObject(requestData, new IsoDateTimeConverter() { DateTimeFormat = "dd.MM.yyyy" });
            request.AddParameter("application/json", serializedRequest, ParameterType.RequestBody);

            var response = client.Execute(request);

            if (response.ResponseStatus != ResponseStatus.Error)
            {
                result.response = JsonConvert.DeserializeObject<RegisterInsuranceResponse.Response>(response.Content);
                result.success = response.IsSuccessful;
                result.response.timestamp = DateTime.Now;
            } 
            else
                result.message = "Сервис не доступен";

            return result;
        }

        private CancelInsuranceResponse SendCancelPolicyRequest(InsurancePoliceCancelRequest requestData)
        {
            var result = new CancelInsuranceResponse();

            var client = new RestClient(GetUrl(_config, false)) { Timeout = -1 };

            var request = new RestRequest(Method.POST);

            var serializedRequest = JsonConvert.SerializeObject(requestData, new IsoDateTimeConverter() { DateTimeFormat = "dd.MM.yyyy" });
            request.AddParameter("application/json", serializedRequest, ParameterType.RequestBody);

            var response = client.Execute(request);

            if (response.ResponseStatus != ResponseStatus.Error)
            {
                result.response = JsonConvert.DeserializeObject<CancelInsuranceResponse.Response>(response.Content);
                result.success = response.IsSuccessful;
            }
            else
                result.message = "Сервис не доступен";

            return result;
        }

        private string GetUrl(OuterServiceCompanyConfig config, bool isRegisterPolicy = true)
        {
            if (string.IsNullOrWhiteSpace(config.ControllerUrl))
                throw new ArgumentNullException(nameof(config.ControllerUrl));

            return isRegisterPolicy?$@"{config.ControllerUrl}/register-insurance": $@"{config.ControllerUrl}/cancel-insurance";
        }

        public void Dispose() => Console.WriteLine($"{nameof(FridomFinanceService)}.Dispose()");
    }
}
