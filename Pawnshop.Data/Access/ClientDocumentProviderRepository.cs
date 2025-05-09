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
    public class ClientDocumentProviderRepository : RepositoryBase, IRepository<ClientDocumentProvider>
    {
        public ClientDocumentProviderRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientDocumentProvider entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ClientDocumentProviders(Code, Name, Abbreviature, NameKaz, AbbreviatureKaz)
VALUES(@Code, @Name, @Abbreviature, @NameKaz, @AbbreviatureKaz)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                entity.PossibleDocumentTypes.ForEach(type => { InsertProviderType(entity, type); });

                transaction.Commit();
            }
        }

        private void InsertProviderType(ClientDocumentProvider provider, ClientDocumentType type)
        {
            UnitOfWork.Session.Execute(@"
INSERT INTO ClientDocumentProviderTypes (ProviderId, TypeId)
SELECT @providerId, @typeId", new { providerId = provider.Id, typeId = type.Id }, UnitOfWork.Transaction);
        }

        public void Update(ClientDocumentProvider entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ClientDocumentProviders SET Code = @Code, Name=@Name, Abbreviature=@Abbreviature, NameKaz=@NameKaz, AbbreviatureKaz=@AbbreviatureKaz
WHERE Id=@id", entity, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute("DELETE FROM ClientDocumentProviderTypes WHERE ProviderId=@id",
                    new { id = entity.Id }, UnitOfWork.Transaction);


                entity.PossibleDocumentTypes.ForEach(type => { InsertProviderType(entity, type); });

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientDocumentProviders SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ClientDocumentProvider Get(int id)
        {
            var item = UnitOfWork.Session.QuerySingleOrDefault<ClientDocumentProvider>(@"
SELECT * 
FROM ClientDocumentProviders
WHERE Id=@id", new { id }, UnitOfWork.Transaction);

            item.PossibleDocumentTypes = UnitOfWork.Session.Query<ClientDocumentType>(@"
SELECT t.* FROM ClientDocumentTypes t
JOIN ClientDocumentProviderTypes pt ON t.Id = pt.TypeId AND pt.ProviderId = @id AND pt.DeleteDate IS NULL", new { id = item.Id }, UnitOfWork.Transaction).ToList();

            return item;
        }

        public async Task<ClientDocumentProvider> GetByCode(string code)
        {
            var parameters = new {Code = code };
            var sqlQuery = @"
                SELECT TOP 1 * FROM ClientDocumentProviders
                WHERE DeleteDate IS NULL
                AND Code = Code";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<ClientDocumentProvider>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public ClientDocumentProvider Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var code = query?.Val<string>("Code");

            var providerId = UnitOfWork.Session.ExecuteScalar<int?>($@"
            SELECT TOP 1 id FROM ClientDocumentProviders WHERE DeleteDate IS NULL AND Code = @code", new { code });

            if (!providerId.HasValue) return null;

            return Get(providerId.Value);
        }

        public List<ClientDocumentProvider> List(ListQuery listQuery, object query = null)
        {
            var list = UnitOfWork.Session.Query<ClientDocumentProvider>(@"SELECT * FROM ClientDocumentProviders", UnitOfWork.Transaction).ToList();

            list.ForEach(item =>
            {
                item.PossibleDocumentTypes = UnitOfWork.Session.Query<ClientDocumentType>(@"
SELECT t.* FROM ClientDocumentTypes t
JOIN ClientDocumentProviderTypes pt ON t.Id = pt.TypeId AND pt.ProviderId = @id AND pt.DeleteDate IS NULL", new { id = item.Id }, UnitOfWork.Transaction)
                    .ToList();
            });
            return list;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ClientDocumentProviders", UnitOfWork.Transaction);
        }

        public async Task<int?> GetMapProviderIdAsync(string value)
        {
            return await UnitOfWork.Session.ExecuteScalarAsync<int?>(@"SELECT cdp.Id
  FROM [map_ClientDocumentProviders] mcdp
  JOIN ClientDocumentProviders cdp ON cdp.Id = mcdp.ProviderId
 WHERE CrmValue = @value",
                new { value }, UnitOfWork.Transaction);
        }
    }
}
