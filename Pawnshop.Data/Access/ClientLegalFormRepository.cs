using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class ClientLegalFormRepository : RepositoryBase, IRepository<ClientLegalForm>
    {
        public ClientLegalFormRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientLegalForm entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ClientLegalForms(Code, Name, Abbreviation, IsIndividual, NameKaz, AbbreviationKaz, CBId, HasIINValidation)
VALUES(@Code, @Name, @Abbreviation, @IsIndividual, @NameKaz, @AbbreviationKaz, @CBId, @HasIINValidation)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientLegalForm entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ClientLegalForms SET Code = @Code, Name=@Name, Abbreviation=@Abbreviation, IsIndividual=@IsIndividual,
NameKaz=@NameKaz, AbbreviationKaz=@AbbreviationKaz, CBId = @CBId, HasIINValidation = @HasIINValidation
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientLegalForms SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ClientLegalForm Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientLegalForm>(@"
SELECT * 
FROM ClientLegalForms
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public async Task<ClientLegalForm> GetByCode(string code)
        {
            var parameters = new { Code = code };
            var sqlQuery = @"
                SELECT top 1 * FROM ClientLegalForms
                WHERE DeleteDate IS NULL
                  AND Code =  @Code";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<ClientLegalForm>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public ClientLegalForm Find(object query)
        {
            if (query == null) 
                throw new ArgumentNullException(nameof(query));

            var code = query?.Val<string>("Code");

            return UnitOfWork.Session.Query<ClientLegalForm>(@"
                    SELECT * FROM ClientLegalForms
                    WHERE Code = @code",
                new { code }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ClientLegalForm> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<ClientLegalForm>(@"SELECT * FROM ClientLegalForms", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ClientLegalForms", UnitOfWork.Transaction);
        }
    }
}
