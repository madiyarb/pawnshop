using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Pawnshop.Data.Access
{
    public class ClientEconomicActivityTypeRepository : RepositoryBase, IRepository<ClientEconomicActivityType>
    {
        public ClientEconomicActivityTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientEconomicActivityType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ClientEconomicActivityTypes(Code, Name, NameAlt, AuthorId, ParentId, HasChild)
                        VALUES(@Code, @Name, @NameAlt, @AuthorId, @ParentId, @HasChild)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientEconomicActivityType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientEconomicActivityTypes SET Code = @Code, Name = @Name, NameAlt = @NameAlt, AuthorId = @AuthorId, ParentId = ParentId, HasChild = @HasChild
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientEconomicActivityType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientEconomicActivityType>(@"
                SELECT * 
                    FROM ClientEconomicActivityTypes
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public List<ClientEconomicActivityType> Find(object query)
        {
            var parentId = query?.Val<int?>("ParentId");

            var pre = parentId.HasValue ? "ParentId = @parentId" : "ParentId IS NULL";

            var condition = $"WHERE {pre}";

            return UnitOfWork.Session.Query<ClientEconomicActivityType>($@"
                WITH HierClientEconomicActivityTypes
                AS ( SELECT Id, Code, ParentId, HasChild, Name, NameAlt
                FROM ClientEconomicActivityTypes
                {condition}
                UNION ALL
                SELECT a.Id, a.Code, a.ParentId, a.HasChild, a.Name, a.NameAlt
                FROM ClientEconomicActivityTypes a
                INNER JOIN HierClientEconomicActivityTypes ON a.ParentId = HierClientEconomicActivityTypes.Id
                )
                SELECT *
                FROM HierClientEconomicActivityTypes", new { parentId }).ToList();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientEconomicActivityTypes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ClientEconomicActivityType> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) 
                throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "Name, Code, NameAlt");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ClientEconomicActivityType>($@"
                SELECT * FROM ClientEconomicActivityTypes
                    {condition} {order} {page}",
                new {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            },
            UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";
            var condition = listQuery.Like(pre, "Name, Code, NameAlt");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM ClientEconomicActivityTypes
                        {condition}", 
                new {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        ClientEconomicActivityType IRepository<ClientEconomicActivityType>.Find(object query)
        {
            throw new NotImplementedException();
        }
    }
}