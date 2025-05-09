using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class RequisiteTypeRepository : RepositoryBase, IRepository<RequisiteType>
    {
        public RequisiteTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(RequisiteType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO RequisiteTypes ( Name, Mask, AuthorId, CreateDate )
VALUES ( @Name, @Mask, @AuthorId, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(RequisiteType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE RequisiteTypes
SET Name = @Name, Mask = @Mask
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE RequisiteTypes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public RequisiteType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<RequisiteType>(@"
SELECT *
FROM RequisiteTypes
WHERE Id = @id", new { id });
        }

        public RequisiteType Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<RequisiteType> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("DeleteDate IS NULL", "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<RequisiteType>($@"
SELECT *
FROM RequisiteTypes
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("DeleteDate IS NULL", "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM RequisiteTypes
{condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int RequisiteTypeId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
    SELECT COUNT(*) 
    FROM ClientRequisites
    WHERE RequisiteTypeId = @RequisiteTypeId", new { RequisiteTypeId });
        }
    }
}