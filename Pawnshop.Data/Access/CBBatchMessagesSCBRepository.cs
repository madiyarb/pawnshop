using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class CBBatchMessagesSCBRepository : RepositoryBase
    {
        public CBBatchMessagesSCBRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int? GetStatusIdByMessage(string message)
        {
            return UnitOfWork.Session.Query<int?>(@$"SELECT StatusId FROM CBBatchMessagesSCB WHERE MessageText LIKE N'%{message}%'", UnitOfWork.Transaction).First();
        }
    }
}
