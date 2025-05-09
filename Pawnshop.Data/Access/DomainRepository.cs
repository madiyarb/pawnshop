using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class DomainRepository : RepositoryBase, IRepository
    {
        public DomainRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Domain entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Code = entity.Code.ToUpper();
            entity.CreateDate = DateTime.Now;
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO Domains ( Code, Name, NameAlt, AuthorId, CreateDate)
                    VALUES ( @Code, @Name, @NameAlt, @AuthorId, @CreateDate)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(Domain entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Domains
                    SET 
                        Name = @Name, 
                        NameAlt = @NameAlt 
                    WHERE Code = @Code", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Domain Get(string code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException($"{nameof(code)} не должен быть пустым или содержать одни пробелы");

            return UnitOfWork.Session.Query<Domain>(@"
                SELECT d.*
                FROM Domains d
                WHERE d.Code=@code",
            new { code }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<Domain> GetAsync(int id)
        {
            var parameters = new { Id = id };
            var sqlQuery = @"SELECT top 1 * FROM DomainValues WHERE Id = @Id";

            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Domain>(sqlQuery, parameters,
                UnitOfWork.Transaction);
        }
        
        public async Task<Domain> GetByCodeAsync(string code)
        {
            var parameters = new { Code = code };
            var sqlQuery = @"SELECT top 1 *
                FROM DomainValues
                WHERE DeleteDate IS NULL
                  AND IsActive = 1
                  AND Code = @Code";

            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Domain>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public Domain Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var code = query?.Val<string>("Code");
            if (code == null)
                throw new InvalidOperationException($"{nameof(code)} не должен быть Null");

            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException($"{nameof(code)} не должен быть пустым");

            return UnitOfWork.Session.Query<Domain>(@"
                SELECT d.*
                FROM Domains d
                WHERE d.Code=@code",
                new { code }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<Domain> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            string pre = string.Empty;
            var condition = listQuery.Like(pre, "d.Name", "d.NameAlt", "d.Code");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "d.Code",
                Direction = SortDirection.Asc
            });

            string pageQuery = listQuery.Page();
            return UnitOfWork.Session.Query<Domain>(@$"
                SELECT d.*
                FROM Domains d
                {condition} {order} {pageQuery}",
                new
                {
                    listQuery.Filter,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            string pre = string.Empty;
            var condition = listQuery.Like(pre, "d.Name", "d.NameAlt", "d.Code");
            return UnitOfWork.Session.ExecuteScalar<int>(@$"
                SELECT COUNT(*)
                FROM Domains d
                {condition}",
                new
                {
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }
    }
}
