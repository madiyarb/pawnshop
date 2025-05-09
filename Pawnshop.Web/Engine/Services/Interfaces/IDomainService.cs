using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Web.Models.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IDomainService
    {
        (List<Domain>, int) GetDomains(ListQuery listQuery);
        List<Domain> GetDomains();
        (List<DomainValue>, int) GetDomainValues(string domainCode, ListQuery listQuery, bool includeDeleted = false);
        List<DomainValue> GetDomainValues(string domainCode, bool includeDeleted = false);
        DomainValue SaveDomainValue(string domainCode, DomainValueDto domainValue);
        void DeleteDomainValue(string domainCode, int domainValueId);
        DomainValue GetDomainValue(string domainCode, int domainValueId, bool throwExceptionOnNotFound = true);
        DomainValue GetDomainValue(string domainCode, string domainValueCode, bool throwExceptionOnNotFound = true);
        Domain GetDomain(string domainCode);
        List<DomainValue> GetDomainValuesForManualBuyoutReasons();
    }
}
