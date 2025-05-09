using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries.Address;
using Dapper;
using System;
using Pawnshop.Core.Queries;
using System.Collections.Generic;

namespace Pawnshop.Data.Access
{
    public class KatoNewRepository : RepositoryBase, IRepository<KatoNew>
    {
        public KatoNewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {
            
        }

        public void Insert(KatoNew entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"INSERT INTO KatoNew (ParentId, KatoCode, Ab, Cd, Ef, Hij, K, NameKaz, NameRus, Nn, Mapped)
                        VALUES(@ParentId, @KatoCode, @Ab, @Cd, @Ef, @Hij, @K, @NameKaz, @NameRus, @Nn, @Mapped)
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(KatoNew entity) 
        {
            using (var transaction = BeginTransaction()) 
            {
                UnitOfWork.Session.Execute(@"UPDATE KatoNew SET
                                                KatoCode = @KatoCode,
                                                Ab = @Ab,
                                                Cd = @Cd,
                                                Ef = @Ef,
                                                Hij = @Hij,
                                                K = @K,
                                                NameKaz = @NameKaz,
                                                NameRus = @NameRus,
                                                Nn = @Nn,
                                                Mapped = @Mapped,
                                                Note = @Note
                                            WHERE Id=@Id AND DeleteDate IS NULL", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int Id)
        {
            throw new NotImplementedException();
        }

        public KatoNew Get(int id) 
        {
            return UnitOfWork.Session.QueryFirstOrDefault<KatoNew>(@"select * FROM KatoNew
                                                where Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public IEnumerable<KatoNew> GetByParentId(int id)
        {
            return UnitOfWork.Session.Query<KatoNew>(@"select * FROM KatoNew
                                                where ParentId = @id", new { id }, UnitOfWork.Transaction);
        }

        public KatoNew Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<KatoNew> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KatoNew> List()
        {
            return UnitOfWork.Session.Query<KatoNew>(@"select * FROM KatoNew
                                                where ParentId = 0", UnitOfWork.Transaction);
        }

        public IEnumerable<KatoNew> ListAll()
        {
            return UnitOfWork.Session.Query<KatoNew>(@"select * FROM [tascredit-uat].[dbo].[KatoNew]", UnitOfWork.Transaction);
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}
