using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class RoleRepository : RepositoryBase, IRepository<Role>
    {
        public RoleRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Role entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Roles ( Name, Permissions ) VALUES ( @Name, @Permissions )
SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Role entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Roles SET Name = @Name, Permissions = @Permissions
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Roles WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Role Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Role>(@"
SELECT *
FROM Roles
WHERE Id = @id", new { id = id });
        }

        public Role Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Role> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Role>($@"
SELECT *
  FROM Roles
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

            var condition = listQuery.Like(string.Empty, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM Roles
{condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int roleId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT COUNT(*)
FROM MemberRoles
WHERE RoleId = @roleId", new { roleId = roleId });
        }

        public List<Role> ListForOnlineFunction()
        {
            return UnitOfWork.Session.Query<Role>(@"SELECT r.*
  FROM Roles r
  JOIN RolesAdditionalParameters rap ON rap.RoleId = r.Id AND rap.DeleteDate IS NULL
 WHERE rap.CanSelectForOnlineFunction = 1",
                null, UnitOfWork.Transaction)
                .ToList();
        }
    }
}