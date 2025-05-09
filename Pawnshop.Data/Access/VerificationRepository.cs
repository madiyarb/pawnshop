using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Verifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class VerificationRepository : RepositoryBase, IRepository<Verification>
    {
        public VerificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Verification entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO Verifications ( OTP, Address, ClientId, AuthorId, ActivationDate, CreateDate, ExpireDate, TryCount, MaxTryCount)
                    VALUES ( @OTP, @Address, @ClientId, @AuthorId, @ActivationDate, @CreateDate, @ExpireDate, @TryCount, @MaxTryCount)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(Verification entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Verifications
                    SET OTP = @OTP, Address = @Address, ClientId = @ClientId, AuthorId = @AuthorId, ActivationDate = @ActivationDate,
                    CreateDate = @CreateDate, ExpireDate = @ExpireDate, TryCount = @TryCount, MaxTryCount = @MaxTryCount
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException("Нет необходимости имплементить удаления для таблицы Verifications");
        }

        public Verification Get(int id)
        {
            return UnitOfWork.Session.Query<Verification>(@"
                SELECT v.*
                FROM Verifications v
                WHERE v.Id=@id",
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Verification GetLastVerification(int clientId)
        {
            return UnitOfWork.Session.Query<Verification>(@"
                SELECT TOP 1 v.*
                FROM Verifications v
                WHERE v.ClientId=@clientId
                ORDER BY v.Id DESC" ,
                new { clientId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Verification Find(object query)
        {
            throw new NotImplementedException("Нет необходимости имплементить метод Find для таблицы Verifications");
        }

        public List<Verification> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException("Нет необходимости имплементить метод List для таблицы Verifications");
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException("Нет необходимости имплементить метод Count для таблицы Verifications");
        }
    }
}
