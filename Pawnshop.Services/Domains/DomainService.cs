using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Domains
{
    public class DomainService : IDomainService
    {
        private readonly DomainRepository _domainRepository;
        private readonly DomainValueRepository _domainValueRepository;
        private readonly ISessionContext _sessionContext;
        private const int MAX_ADDITIONAL_DATA_SERIALIZED_LENGTH = 512;
        public DomainValue DomainValue { get; private set; }
        public DomainService(DomainRepository domainRepository, DomainValueRepository domainValueRepository, ISessionContext sessionContext)
        {
            _domainRepository = domainRepository;
            _domainValueRepository = domainValueRepository;
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

        public async Task<Domain> GetAsync(int id)
        {
            return await _domainRepository.GetAsync(id);
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

        public DomainValue getDomainCodeById(int? domainValueId)
        {
            if (domainValueId == null)
                throw new ArgumentNullException(nameof(domainValueId));

            return _domainValueRepository.Get(domainValueId??0);
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
    }
}
