using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries.Address;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Dictionaries;
using Dapper;
using Pawnshop.Core.Queries;
using System.Transactions;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class LanguagesRepository : RepositoryBase, IRepository<Language>
    {
        public LanguagesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Language entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO Languages (Code, Name, CreateDate, AuthorId)
VALUES(@Code, @Name, @CreateDate, @AuthorId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(Language entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Languages SET Code = @Code, Name = @Name, CreateDate = @CreateDate, AuthorId = @AuthorId
WHERE Id=@id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Languages SET DeleteDate = dbo.GETASTANADATE()
WHERE Id=@id", new { id } , UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Language Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Language>(@"
SELECT * 
FROM Languages
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public Language Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Language> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<Language>(@"SELECT * FROM Languages WHERE DeleteDate IS NULL", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM Languages WHERE DeleteDate IS NULL", UnitOfWork.Transaction);
        }
    }
}
