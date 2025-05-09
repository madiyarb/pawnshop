using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Domains
{
    public interface IDomainService
    {
        (List<Domain>, int) GetDomains(ListQuery listQuery);
        List<Domain> GetDomains();
        (List<DomainValue>, int) GetDomainValues(string domainCode, ListQuery listQuery, bool includeDeleted = false);
        List<DomainValue> GetDomainValues(string domainCode, bool includeDeleted = false);
        DomainValue GetDomainValue(string domainCode, int domainValueId, bool throwExceptionOnNotFound = true);
        DomainValue GetDomainValue(string domainCode, string domainValueCode, bool throwExceptionOnNotFound = true);
        Domain GetDomain(string domainCode);
        Task<Domain> GetAsync(int id);
        DomainValue getDomainCodeById(int? domainValueId);
    }
}
