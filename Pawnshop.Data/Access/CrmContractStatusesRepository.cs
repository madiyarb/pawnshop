using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Crm;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class CrmStatusesRepository : RepositoryBase
    {
        public CrmStatusesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public CrmStatusType FindStatusType(object query)
        {
            var predicate = "WHERE DeleteDate IS NULL";

            var id = query?.Val<int?>("Id");
            var code = query?.Val<string>("Code");

            predicate += id.HasValue ? " AND Id = @id" : string.Empty;
            predicate += !string.IsNullOrEmpty(code) ? " AND Code = @code" : string.Empty;

            return UnitOfWork.Session.Query<CrmStatusType>($@"
                SELECT *
	            FROM CrmStatusTypes
                    {predicate}
            ", new { id, code }).FirstOrDefault();
        }

        public CrmStatus FindStatus(object query)
        {
            var predicate = "WHERE DeleteDate IS NULL";

            var id = query?.Val<int?>("Id");
            var crmName = query?.Val<string>("CrmName");
            var displayName = query?.Val<string>("DisplayName");
            var statusTypeId = query?.Val<int?>("StatusTypeId");

            predicate += id.HasValue ? " AND Id = @id" : string.Empty;
            predicate += !string.IsNullOrEmpty(crmName) ? " AND CrmName = @crmName " : string.Empty;
            predicate += !string.IsNullOrEmpty(displayName) ? " AND DisplayName = @displayName " : string.Empty;
            predicate += statusTypeId.HasValue ? " AND StatusTypeId = @statusTypeId " : string.Empty;


            return UnitOfWork.Session.Query<CrmStatus>($@"
                SELECT *
	            FROM CrmStatuses
                    {predicate}
            ", new { id, crmName, displayName, statusTypeId }).FirstOrDefault();
        }
    }
}
