using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.Insurance;

namespace Pawnshop.Services.Insurance
{
    public class InsuranceOnlineRequestService : BaseService<InsuranceOnlineRequest>, IInsuranceOnlineRequestService
    {
        private readonly IContractService _contractService;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly ClientDocumentTypeRepository _clientDocumentTypeRepository;

        public InsuranceOnlineRequestService(IRepository<InsuranceOnlineRequest> repository,
            ContractService contractService,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            ClientDocumentTypeRepository clientDocumentTypeRepository) : base(repository)
        {
            _contractService = contractService;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _clientDocumentTypeRepository = clientDocumentTypeRepository;
        }

        public InsuranceOnlineRequest Save(InsuranceRequestData requestData)
        {
            if (requestData is null)
                throw new PawnshopApplicationException("Данные пришедшие с сайта пусты");

            var contractNumber = requestData.ContractNumber;
            int index = contractNumber.LastIndexOf("-");

            if (index >= 0)
                contractNumber = contractNumber.Substring(0, index);

            var contract = _contractService.Find(new ContractFilter() { ContractNumber = contractNumber });

            if (contract is null)
                throw new PawnshopApplicationException($"Контракт с номером {requestData.ContractNumber} не найден");

            if (contract.ContractClass == ContractClass.CreditLine && contract.CreatedInOnline)
            {
                contract = _contractService.GetNonCreditLineByNumberAsync(contractNumber).Result;

                if (contract is null)
                    throw new PawnshopApplicationException($"Контракт с номером {requestData.ContractNumber} не найден");
            }

            contract = _contractService.Get(contract.Id);

            var actualPoliceRequest = GetActualPoliceRequest(contract.Id);

            ValidateRequestData(requestData, actualPoliceRequest.RequestData, contract);

            var insuranceOnlineRequst = new InsuranceOnlineRequest()
            {
                ContractId = contract.Id,
                CreateDate = DateTime.Now,
                RequestData = requestData
            };

            if (contract.SettingId.HasValue && contract.ProductTypeId.HasValue &&
                contract.ProductType.Code == Constants.PRODUCT_BUYCAR && contract.Status == ContractStatus.Draft)
                contract.Status = ContractStatus.AwaitForInitialFeeOpen;
            else if (contract.Status != ContractStatus.Signed && !contract.CreatedInOnline)
                contract.Status = ContractStatus.AwaitForInsuranceSend;

            using (var transaction = _repository.BeginTransaction())
            {
                base.Save(insuranceOnlineRequst);

                actualPoliceRequest.OnlineRequestId = insuranceOnlineRequst.Id;
                _insurancePoliceRequestService.Save(actualPoliceRequest);

                _contractService.Save(contract);

                transaction.Commit();
            }

            if (actualPoliceRequest.OnlineRequestId is null)
                throw new PawnshopApplicationException(
                    "Для запроса в СК не был сохранен онлайн запрос");

            return insuranceOnlineRequst;
        }

        private InsurancePoliceRequest GetActualPoliceRequest(int contractId)
        {
            var actualPoliceRequest = _insurancePoliceRequestService.GetNewPoliceRequest(contractId);

            if (actualPoliceRequest is null)
                throw new PawnshopApplicationException(
                    "Для контракта не найден актуальный запрос в страховую компанию");

            if (actualPoliceRequest.RequestData is null)
                throw new PawnshopApplicationException(
                    "Для контракта не найдены данные, которые должны быть отправлены в страховую компанию");

            return actualPoliceRequest;
        }

        private void ValidateRequestData(InsuranceRequestData requestData, InsuranceRequestData policeRequestData, Contract contract)
        {
            if (requestData != null)
            {
                var model = new InsuranceOnlineRequestValidateModel()
                {
                    Client = contract.Client,
                    ClientDocument = _insurancePoliceRequestService.GetLastClientDocumentForInsurance(contract.Client),
                    ClientAddress = _insurancePoliceRequestService.GetActualAdressForInsurance(contract.Client),
                    ClientContact = _insurancePoliceRequestService.GetDefaultClientContact(contract.Client),
                    Contract = contract,
                    PoliceRequestData = policeRequestData
                };

                var errors = new List<string>
                {
                    "Не совпадение данных:"
                };


                foreach (var property in typeof(InsuranceRequestData).GetProperties())
                {
                    var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                        .Cast<DisplayNameAttribute>().FirstOrDefault();

                    var value = property.GetValue(requestData);
                    var valueFromDb = IsNotEqualGetValueFromDb(value, property.Name, model);

                    if (valueFromDb != null)
                    {
                        if (property.Name == nameof(InsuranceRequestData.DocumentType))
                            errors.Add($"{attribute.DisplayName}: на сайте {GetDocumentTypeName(GetDocumentType((int)value))}, на договоре {GetDocumentTypeName(valueFromDb.ToString())}");
                        else
                            errors.Add($"{attribute.DisplayName}: на сайте {value}, на договоре {valueFromDb}");
                    }
                }

                if (errors.Count > 1)
                    throw new PawnshopApplicationException(errors.ToArray());
            }
        }


        public object IsNotEqualGetValueFromDb(object value, string propertyName, InsuranceOnlineRequestValidateModel model)
        {
            var valueFromDb = GetValueFromDb(propertyName, model);

            if (valueFromDb is null)
                return null;

            if (value is null)
            {
                return value == valueFromDb ? null : valueFromDb;
            }
            else
            {
                if (propertyName == nameof(InsuranceRequestData.DocumentType))
                    return CompareDocumentTypes((int)value, valueFromDb.ToString()) ? null : valueFromDb;

                if (value.GetType() == typeof(DateTime))
                    return ((DateTime)value).Date.Equals(valueFromDb) ? null : $"{valueFromDb:dd.MM.yyyy}";

                return value.Equals(valueFromDb) ? null : valueFromDb;
            }
        }

        public object GetValueFromDb(string propertyName, InsuranceOnlineRequestValidateModel model)
        {
            return propertyName switch
            {
                nameof(InsuranceRequestData.FullName) => model.Client.FullName,
                nameof(InsuranceRequestData.IdentityNumber) => model.Client.IdentityNumber,
                nameof(InsuranceRequestData.BirthDay) => model.Client.BirthDay,
                nameof(InsuranceRequestData.DocumentNumber) => model.ClientDocument.Number,
                nameof(InsuranceRequestData.DocumentDate) => model.ClientDocument.Date.Value.Date,
                nameof(InsuranceRequestData.Provider) => model.ClientDocument.Provider.Name,
                nameof(InsuranceRequestData.DocumentType) => model.ClientDocument.DocumentType.Code,
                nameof(InsuranceRequestData.ContractNumber) => model.PoliceRequestData.ContractNumber,
                nameof(InsuranceRequestData.Address) => model.ClientAddress.FullPathRus,
                nameof(InsuranceRequestData.PhoneNumber) => model.ClientContact.Address,
                nameof(InsuranceRequestData.Email) => model.Client.Email,
                nameof(InsuranceRequestData.IsResident) => Convert.ToInt32(model.Client.IsResident),
                nameof(InsuranceRequestData.InsuranceStartDate) => DateTime.Now.Date,
                nameof(InsuranceRequestData.InsuranceEndDate) => model.PoliceRequestData.InsuranceEndDate.Date,
                nameof(InsuranceRequestData.InsurancePeriod) => model.PoliceRequestData.InsurancePeriod,
                nameof(InsuranceRequestData.InsuranceAmount) => model.PoliceRequestData.InsuranceAmount,
                nameof(InsuranceRequestData.InsurancePremium) => model.PoliceRequestData.InsurancePremium,
                nameof(InsuranceRequestData.InsuranceRate) => model.PoliceRequestData.InsuranceRate,
                nameof(InsuranceRequestData.InsuranceType) => Constants.INSURANCE_TYPE_CODE,
                _ => null
            };
        }

        private bool CompareDocumentTypes(int documentType, string value) =>

            documentType switch
            {
                1 => value.Equals(Constants.IDENTITYCARD),
                2 => value.Equals(Constants.RESIDENCE),
                3 => value.Equals(Constants.PASSPORTKZ),
                _ => throw new NotImplementedException(),
            };

        private string GetDocumentType(int documentType) =>
            documentType switch
            {
                1 => Constants.IDENTITYCARD,
                2 => Constants.RESIDENCE,
                3 => Constants.PASSPORTKZ,
                _ => throw new NotImplementedException(),
            };

        private string GetDocumentTypeName(string code)
        {
            var documentType = _clientDocumentTypeRepository.Find(new { Code = code });

            if (documentType != null)
                return documentType.Name;
            else return code;
        }
    }
}