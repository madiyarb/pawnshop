using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Pawnshop.Services.Realties
{
    public class RealtyService : IRealtyService
    {
        private readonly RealtyRepository _realtyRepository;
        private readonly IDomainService _domainService;
        private readonly IClientIncomeService _clientIncomeService;
        private readonly RealtyDocumentsRepository _realtyDocumentsRepository;
        private readonly RealtyAddressRepository _realtyAddressRepository;
        private readonly PositionEstimatesRepository _positionEstimatesRepository;
        private readonly ISessionContext _sessionContext;

        public RealtyService(RealtyRepository realtyRepository,
                             IDomainService domainService,
                             IClientIncomeService clientIncomeService,
                             RealtyDocumentsRepository realtyDocumentsRepository,
                             RealtyAddressRepository realtyAddressRepository,
                             PositionEstimatesRepository positionEstimatesRepository,
                             ISessionContext sessionContext)
        {
            _realtyRepository = realtyRepository;
            _domainService = domainService;
            _clientIncomeService = clientIncomeService;
            _realtyDocumentsRepository = realtyDocumentsRepository;
            _realtyAddressRepository = realtyAddressRepository;
            _positionEstimatesRepository = positionEstimatesRepository;
            _sessionContext = sessionContext;
        }

        public ListModel<Realty> ListWithCount(ListQuery listQuery)
        {
            return new ListModel<Realty>
            {
                List = _realtyRepository.List(listQuery),
                Count = _realtyRepository.Count(listQuery)
            };
        }

        public Realty Save(Realty realty)
        {
            ValidateRealtyForSave(realty);
            using (var transaction = _realtyRepository.BeginTransaction())
            {
                if (realty.Id > 0)
                {
                    if (!_sessionContext.HasPermission(Permissions.RealtyUpdate))
                        throw new PawnshopApplicationException("Нет прав на обновление позиции недвижимости");
                    UpdateRealty(realty);
                }
                else
                {
                    _realtyRepository.Insert(realty);
                    realty.Address.Id = realty.Id;
                    _realtyAddressRepository.Insert(realty.Address);
                    foreach (var doc in realty.RealtyDocuments)
                    {
                        doc.RealtyId = realty.Id;
                        doc.AuthorId = _sessionContext.UserId;
                        doc.CreateDate = DateTime.Now;
                        _realtyDocumentsRepository.Insert(doc);
                    }
                }
                transaction.Commit();
            }
            return realty;
        }

        private void ValidateRealtyForSave(Realty realty)
        {
            realty.RealtyType = _domainService.getDomainCodeById(realty.RealtyTypeId);

            if (realty.RealtyDocuments == null)
                throw new PawnshopApplicationException("Поле документы обязательно при заполнении");

            if (!realty.RealtyDocuments.Any())
                throw new PawnshopApplicationException("Поле документ обязательно при заполнении");

            if (realty.Address.AteId == 0 || realty.Address.GeonimId == 0)
                throw new PawnshopApplicationException("Адрес не заполнен полностью");

            if (String.IsNullOrWhiteSpace(realty.Address.BuildingNumber))
                throw new PawnshopApplicationException("Номер дома обязательно к заполнению");

            if (realty.RealtyType.Code == Constants.REALTY_APPARMENT && String.IsNullOrWhiteSpace(realty.Address.AppartmentNumber))
                throw new PawnshopApplicationException("Номер квартиры обязателен для заполнения");

            foreach (var doc in realty.RealtyDocuments)
            {
                if (doc.DocumentTypeId == 0)
                    throw new PawnshopApplicationException("Тип документа обязателен для заполнения");
                if (String.IsNullOrEmpty(doc.Number) || doc.Date == null)
                    throw new PawnshopApplicationException("Номер и дата документа должны быть обязательными");
            }
        }

        private void UpdateRealty(Realty realty)
        {
            _realtyRepository.Update(realty);
            realty.Address.Id = realty.Id;

            var existingAddress = _realtyAddressRepository.Get(realty.Id);
            if (existingAddress != null && existingAddress.Id > 0)
                _realtyAddressRepository.Update(realty.Address);
            else
                _realtyAddressRepository.Insert(realty.Address);

            foreach (var doc in realty.RealtyDocuments)
            {
                if (doc.Id > 0)
                    _realtyDocumentsRepository.Update(doc);
                else
                {
                    doc.RealtyId = realty.Id;
                    doc.AuthorId = _sessionContext.UserId;
                    doc.CreateDate = DateTime.Now;
                    _realtyDocumentsRepository.Insert(doc);
                }
            }

            // удаление существующих документов при удалении
            var existingDocList = _realtyDocumentsRepository.GetDocumentsForRealty(realty.Id);
            foreach(var doc in existingDocList)
            {
                if(!realty.RealtyDocuments.Exists( inDoc => inDoc.Id == doc.Id))
                {
                    _realtyDocumentsRepository.Delete(doc.Id);
                }
            }
        }

        public void Delete(int id)
        {
            var count = _realtyRepository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить позицию, так как она привязана к позиции договора");
            }
            _realtyRepository.Delete(id);
            _realtyAddressRepository.Delete(id);
            _realtyDocumentsRepository.DeleteForRealty(id);
        }

        public Realty Get(int id)
        {
            var realty = _realtyRepository.Get(id);
            realty.Address = _realtyAddressRepository.Get(id);
            realty.RealtyDocuments = _realtyDocumentsRepository.GetDocumentsForRealty(id);
            realty.RealtyType = _domainService.getDomainCodeById(realty.RealtyTypeId);
            if (realty == null)
                throw new NullReferenceException($"Недвижимость с Id {id} не найдена");
            return realty;
        }

        public void Validate(Realty realty)
        {
            var validationErrors = new List<string>();

            foreach (var property in typeof(Realty).GetProperties())
            {
                var propertyValue = property.GetValue(realty);

                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                    .Cast<DisplayNameAttribute>().FirstOrDefault();

                bool isEmptyStringOrNull = propertyValue == null;
                if (propertyValue != null)
                {
                    if (property.Name == Constants.Rca && !isEmptyStringOrNull)
                        ValidateRealtyFields(Constants.RCA_REGEX, attribute.DisplayName, propertyValue.ToString(), validationErrors);

                    if (property.Name == Constants.CadastralNumber && !isEmptyStringOrNull)
                        ValidateRealtyFields(Constants.CADASTRAL_NUMBER_REGEX, attribute.DisplayName, propertyValue.ToString(), validationErrors);
                }
            }

            if (validationErrors.Count > 0)
                throw new PawnshopApplicationException(validationErrors.ToArray());
        }

        private void ValidateRealtyFields(string regexStr, string fieldName, string value, List<string> validationErrors)
        {
            var regex = new Regex(regexStr);

            if (!regex.IsMatch(value))
                validationErrors.Add($"Неподдерживаемые символы в поле {fieldName}: \"{value}\"");
        }

        public List<DomainValue> GetRealtyTypes()
        {
            return _domainService.GetDomainValues(Constants.REALTY_TYPE);
        }

        public List<DomainValue> GetRealtyPurpose()
        {
            return _domainService.GetDomainValues(Constants.REALTY_PURPOSE);
        }

        public List<DomainValue> GetRealtyWallMaterial()
        {
            return _domainService.GetDomainValues(Constants.REALTY_WALL_MATERIAL);
        }

        public List<DomainValue> GetRealtyLightning()
        {
            return _domainService.GetDomainValues(Constants.REALTY_LIGHTNING);
        }

        public List<DomainValue> GetRealtyColdWaterSupply()
        {
            return _domainService.GetDomainValues(Constants.REALTY_COLD_WATER_SUPPLY);
        }

        public List<DomainValue> GetRealtyGasSupply()
        {
            return _domainService.GetDomainValues(Constants.REALTY_GAS_SUPPLY);
        }

        public List<DomainValue> GetRealtySanitation()
        {
            return _domainService.GetDomainValues(Constants.REALTY_SANITATION);
        }

        public List<DomainValue> GetRealtyHotWaterSupply()
        {
            return _domainService.GetDomainValues(Constants.REALTY_HOT_WATER_SUPPLY);
        }

        public List<DomainValue> GetRealtyHeating()
        {
            return _domainService.GetDomainValues(Constants.REALTY_HEATING);
        }

        public List<DomainValue> GetRealtyPhoneConnection()
        {
            return _domainService.GetDomainValues(Constants.REALTY_PHONE_CONNECTION);
        }

        public NotionalRate GetVpm()
        {
            return _clientIncomeService.GetNotionalRate(Constants.NOTIONAL_RATE_TYPES, Constants.NOTIONAL_RATE_TYPES_VPM);
        }
    }
}
