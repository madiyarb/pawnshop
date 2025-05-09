using System.Collections.Generic;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.ManualUpdate;

namespace Pawnshop.Data.Access
{
    public class ManualUpdateHistoryRepository : RepositoryBase
    {
        public ManualUpdateHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ManualUpdateModel entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ManualUpdateHistory (UserId, CreateDate, CategoryId, SelectQuery, SelectResult, UpdateResult, UpdateQuery)
VALUES (@UserId, @CreateDate, @CategoryId, @SelectQuery, @SelectResult, @UpdateResult, @UpdateQuery)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}