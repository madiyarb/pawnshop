using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class LoanSubjectRepository : RepositoryBase, IRepository<LoanSubject>
    {
        public LoanSubjectRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(LoanSubject entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO LoanSubjects 
                        (Name, NameAlt, Code, AuthorId, CreateDate, CBId) 
                    VALUES 
                        (@Name, @NameAlt, @Code, @AuthorId, @CreateDate, @CBId)
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(LoanSubject entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE LoanSubjects SET Name = @Name, NameAlt = @NameAlt, Code = @Code, CBId = @CBId WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"declare @cid int;
set @cid = (select ContractId from ContractLoanSubjects where id = @id)
if(select Status from Contracts where Id = @cid) < 30
begin
	UPDATE ContractLoanSubjects SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id
end", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public LoanSubject Get(int id)
        {
            return UnitOfWork.Session.Query<LoanSubject, User, LoanSubject>(@"
SELECT ls.*, u.*
  FROM LoanSubjects ls
LEFT JOIN Users u ON u.Id = ls.AuthorId
WHERE ls.Id = @id", (h, u) =>
            {
                h.Author = u;
                return h;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public LoanSubject Find(object query)
        {
            throw new NotImplementedException();
        }

        public LoanSubject GetByCode(string code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Аргумент не должен быть пустым или содержать одни пробелы", nameof(code));

            return UnitOfWork.Session.Query<LoanSubject>($@"
                SELECT ls.*
                    FROM LoanSubjects ls
                WHERE ls.Code = @code AND ls.DeleteDate IS NULL", new { code },
                UnitOfWork.Transaction).SingleOrDefault();
        }
        
        public async Task<LoanSubject> GetByCodeAsync(string code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Аргумент не должен быть пустым или содержать одни пробелы", nameof(code));

            var parameters = new { Code = code };
            var sqlQuery = @"
                SELECT top 1 *
                FROM LoanSubjects
                WHERE DeleteDate IS NULL
                  AND Code = @Code";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<LoanSubject>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public List<LoanSubject> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "ls.DeleteDate IS NULL";

            var condition = listQuery.Like(pre);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<LoanSubject, User, LoanSubject>($@"
SELECT ls.*, u.*
  FROM LoanSubjects ls
LEFT JOIN Users u ON u.Id = ls.AuthorId
{condition} {order} {page}", (h, u) =>
            {
                h.Author = u;
                return h;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "ls.DeleteDate IS NULL";

            var condition = listQuery.Like(pre);

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM LoanSubjects ls
{condition}", new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(q)
FROM (
SELECT COUNT(*)
FROM LoanRequiredSubjects
WHERE SubjectId = @id
SELECT COUNT(*) as q
FROM ContractLoanSubjects
WHERE SubjectId = @id) as t", new { id });
        }
    }
}