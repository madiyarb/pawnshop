using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Domains;
using Pawnshop.Web.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services
{
    public class DomainService : IDomainService
    {
        private readonly DomainRepository _domainRepository;
        private readonly DomainValueRepository _domainValueRepository;
        private readonly ISessionContext _sessionContext;
        private readonly EventLog _eventLog;
        private const int MAX_ADDITIONAL_DATA_SERIALIZED_LENGTH = 512;
        public DomainValue DomainValue { get; private set; }
        public DomainService(DomainRepository domainRepository, DomainValueRepository domainValueRepository, ISessionContext sessionContext, EventLog eventLog)
        {
            _domainRepository = domainRepository;
            _domainValueRepository = domainValueRepository;
            _eventLog = eventLog;
            _sessionContext = sessionContext;
        }

        public (List<Domain>, int) GetDomains(ListQuery listQuery)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            return (_domainRepository.List(listQuery, null), _domainRepository.Count(listQuery, null));
        }

        public List<Domain> GetDomains()
        {
            return _domainRepository.List(new ListQuery { Page = null });
        }

        public Domain GetDomain(string domainCode)
        {
            if (domainCode == null)
                throw new ArgumentNullException(nameof(domainCode));
            if (domainCode.Trim() == string.Empty)
                throw new ArgumentException($"Параметр не может быть пустым", nameof(domainCode));

            Domain domain = _domainRepository.Get(domainCode);
            if (domain == null)
                throw new PawnshopApplicationException($"Домен с кодом '{domainCode}' не найден");

            return domain;
        }
        public List<DomainValue> GetDomainValues(string domainCode, bool includeDeleted = false)
        {
            if (domainCode == null)
                throw new ArgumentNullException(nameof(domainCode));
            if (domainCode.Trim() == string.Empty)
                throw new ArgumentException($"Параметр не может быть пустым", nameof(domainCode));

            Domain domain = GetDomain(domainCode);
            var query = new { DomainCode = domain.Code, IncludeDeleted = includeDeleted };
            var list = _domainValueRepository.List(new ListQuery { Page = null }, query).OfType<DomainValue>().ToList();
            if (domainCode == "POSITION_TYPE")
            {
                var selfEmployer = list.Find(x => x.Code == "FREELANCER");
                if (selfEmployer != null)
                {
                    list.Remove(selfEmployer);
                    list.Insert(0, selfEmployer);
                }
            }
            return list;
        }

        public (List<DomainValue>, int) GetDomainValues(string domainCode, ListQuery listQuery, bool includeDeleted = false)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            if (domainCode == null)
                throw new ArgumentNullException(nameof(domainCode));
            if (domainCode.Trim() == string.Empty)
                throw new ArgumentException($"Параметр не может быть пустым", nameof(domainCode));

            Domain domain = GetDomain(domainCode);
            var query = new { DomainCode = domain.Code, IncludeDeleted = includeDeleted };
            return (_domainValueRepository.List(listQuery, query), _domainValueRepository.Count(listQuery, query));
        }

        public DomainValue GetDomainValue(string domainCode, int domainValueId, bool throwExceptionOnNotFound = true)
        {
            if (domainCode == null)
                throw new ArgumentNullException(nameof(domainCode));
            if (domainCode.Trim() == string.Empty)
                throw new ArgumentException($"Параметр не может быть пустым", nameof(domainCode));

            Domain domain = GetDomain(domainCode);
            DomainValue domainValueFromDB = _domainValueRepository.Get(domainValueId);
            if (throwExceptionOnNotFound && domainValueFromDB == null)
                throw new PawnshopApplicationException($"Значение домена с Id {domainValueId} не найдено");

            if (domainValueFromDB.DomainCode != domain.Code)
                throw new PawnshopApplicationException($"Значение домена с Id {domainValueId} не принадлежит домену с Code {domain.Code}");

            return domainValueFromDB;
        }

        public DomainValue GetDomainValue(string domainCode, string domainValueCode, bool throwExceptionOnNotFound = true)
        {
            if (domainValueCode == null)
                throw new ArgumentNullException(nameof(domainValueCode));

            if (string.IsNullOrWhiteSpace(domainValueCode))
                throw new ArgumentException($"{nameof(domainValueCode)} не должен быть пустым или содержать одни пробелы");

            if (domainCode == null)
                throw new ArgumentNullException(nameof(domainCode));
            if (domainCode.Trim() == string.Empty)
                throw new ArgumentException($"Параметр не может быть пустым", nameof(domainCode));

            Domain domain = GetDomain(domainCode);
            DomainValue domainValueFromDB = _domainValueRepository.GetByCodeAndDomainCode(domainValueCode, domain.Code);
            if (throwExceptionOnNotFound && domainValueFromDB == null)
                throw new PawnshopApplicationException($"Значение домена с кодом '{domain.Code}'.'{domainValueCode}' не найдено");

            return domainValueFromDB;
        }

        public DomainValue SaveDomainValue(string domainCode, DomainValueDto domainValue)
        {
            if (domainValue == null)
                throw new ArgumentNullException(nameof(domainValue));

            if (string.IsNullOrWhiteSpace(domainCode))
                throw new ArgumentException("Аргумента не должнен быть пустым", nameof(domainCode));

            Domain domain = GetDomain(domainCode);
            ValidateDomainValue(domainValue);
            DomainValue domainValueWithTheSameCodeFromDB = GetDomainValue(domainCode, domainValue.Code, false);
            DomainValue domainValueFromDB = null;
            if (domainValue.Id == 0)
            {
                if (domainValueWithTheSameCodeFromDB != null)
                    throw new PawnshopApplicationException($"Значение домена с кодом '{domainCode}'.'{domainValue.Code}' уже существует");

                domainValueFromDB = new DomainValue
                {
                    AuthorId = _sessionContext.UserId,
                    AdditionalData = domainValue.AdditionalDataSerialized,
                    IsActive = domainValue.IsActive.Value,
                    NameAlt = domainValue.NameAlt,
                    Name = domainValue.Name,
                    Code = domainValue.Code,
                    DomainCode = domain.Code
                };
                _domainValueRepository.Insert(domainValueFromDB);
                _eventLog.Log(EventCode.DomainValueCreated, EventStatus.Success, EntityType.DomainValue, domainValue.Id, JsonConvert.SerializeObject(domainValue));
            }
            else
            {
                domainValueFromDB = GetDomainValue(domainCode, domainValue.Id);
                if (domainValueWithTheSameCodeFromDB != null && domainValueFromDB.Id != domainValueWithTheSameCodeFromDB.Id)
                    throw new PawnshopApplicationException($"Значение домена с кодом '{domainCode}'.'{domainValue.Code}' уже существует");

                domainValueFromDB.AdditionalData = domainValue.AdditionalDataSerialized;
                domainValueFromDB.IsActive = domainValue.IsActive.Value;
                domainValueFromDB.NameAlt = domainValue.NameAlt;
                domainValueFromDB.Name = domainValue.Name;
                domainValueFromDB.Code = domainValue.Code;
                _domainValueRepository.Update(domainValueFromDB);
                _eventLog.Log(EventCode.DomainValueUpdated, EventStatus.Success, EntityType.DomainValue, domainValue.Id, JsonConvert.SerializeObject(domainValue));
            }


            return domainValueFromDB;
        }

        public void DeleteDomainValue(string domainCode, int domainValueId)
        {
            if (domainCode == null)
                throw new ArgumentNullException(nameof(domainCode));
            if (domainCode.Trim() == string.Empty)
                throw new ArgumentException($"Параметр не может быть пустым", nameof(domainCode));

            DomainValue domainValueFromDB = GetDomainValue(domainCode, domainValueId);
            _domainValueRepository.Delete(domainValueFromDB.Id);
            _eventLog.Log(EventCode.DomainValueDeleted, EventStatus.Success, EntityType.DomainValue, domainValueFromDB.Id);
        }

        private void ValidateDomainValue(DomainValueDto domainValue)
        {
            if (domainValue == null)
                throw new ArgumentNullException(nameof(domainValue));

            var errorMessages = new List<string>();
            if (!domainValue.IsActive.HasValue)
                errorMessages.Add($"Поле {nameof(domainValue.IsActive)} не должно быть пустым");

            if (domainValue.AdditionalData != null)
                if (domainValue.AdditionalDataSerialized.Length > MAX_ADDITIONAL_DATA_SERIALIZED_LENGTH)
                    errorMessages.Add($"Обнаружен слишком большой объект в поле {nameof(domainValue.AdditionalData)}, максимальная длина сериализованного в JSON объекта не должна превышать {MAX_ADDITIONAL_DATA_SERIALIZED_LENGTH} символов");

            if (errorMessages.Count > 0)
                throw new PawnshopApplicationException(errorMessages.ToArray());
        }

        public List<DomainValue> GetDomainValuesForManualBuyoutReasons()
        {
            return _domainValueRepository.ListOnlyManualBuyoutReasons();
        }
    }
}
