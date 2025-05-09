using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Type = Pawnshop.Data.Models.AccountingCore.Type;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class TypeRepository : RepositoryBase, IRepository<Type>
    {
        public TypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Type entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO TypesHierarchy ( Code, Name, NameAlt, TypeGroup, ParentId, CreateDate, AuthorId )
VALUES ( @Code, @Name, @NameAlt, @TypeGroup, @ParentId, @CreateDate, @AuthorId  )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(Type entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE TypesHierarchy
SET Code = @Code, Name = @Name, NameAlt = @NameAlt, TypeGroup = @TypeGroup, ParentId = @ParentId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public async void Delete(int id)
        {
            using var transaction = BeginTransaction();

            await UnitOfWork.Session.ExecuteAsync(@"UPDATE TypesHierarchy SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public Type GetByCode(string code)
        {
            var type = UnitOfWork.Session.QuerySingleOrDefault<Type>(@"
                SELECT *
                FROM TypesHierarchy
                WHERE Code = @code", new { code }, UnitOfWork.Transaction);

            if (type.ParentId.HasValue)
                type.Parent = Get(type.ParentId.Value);

            return type;
        }
        
        public async Task<Type> GetByCodeAsync(string code)
        {
            var type = await UnitOfWork.Session.QueryFirstOrDefaultAsync<Type>(@"
                SELECT *
                FROM TypesHierarchy
                WHERE Code = @code", new { code }, UnitOfWork.Transaction);

            if (type.ParentId.HasValue)
            {
                type.Parent = await GetAsync(type.ParentId.Value);
            }

            return type;
        }

        public Type Get(int id)
        {
            var type = UnitOfWork.Session.QuerySingleOrDefault<Type>(@"
SELECT *
FROM TypesHierarchy
WHERE Id = @id", new { id }, UnitOfWork.Transaction);

            if (type.ParentId.HasValue) type.Parent = Get(type.ParentId.Value);

            return type;
        }

        public async Task<Type> GetAsync(int id)
        {
            var type = await UnitOfWork.Session.QueryFirstOrDefaultAsync<Type>(@"
SELECT *
FROM TypesHierarchy
WHERE Id = @id", new { id }, UnitOfWork.Transaction);

            if (type.ParentId.HasValue) type.Parent = await GetAsync(type.ParentId.Value);

            return type;
        }

        public Type Find(object query)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Type>(@"
SELECT TOP 1 *
FROM TypesHierarchy");
        }

        public List<Type> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Code");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Code",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Type>($@"
SELECT *
FROM TypesHierarchy
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Code");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM TypesHierarchy
{condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(TypesHierarchyCounts.TypesHierarchyCount)
FROM (
    SELECT 1 as TypesHierarchyCount --WHERE @id = @id
) TypesHierarchyCounts", new { id });
        }

        public async Task<Type> FindAsync(object query)
        {
            if (query == null)
                throw new ArgumentNullException("The query cannot be empty!");

            var code = query?.Val<string>("Code");

            var condition = "WHERE DeleteDate IS NULL AND Code = @code";

            return await UnitOfWork.Session.QuerySingleOrDefaultAsync<Type>($@"
SELECT TOP 1 *
  FROM TypesHierarchy
 {condition}",
                new { code }, UnitOfWork.Transaction);
        }
    }
}