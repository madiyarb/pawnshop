using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ClientAssetRepository : RepositoryBase, IRepository<ClientAsset>
    {
        public ClientAssetRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientAsset entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO ClientAssets
                    (ClientId, AssetTypeId, Count, AuthorId, CreateDate, DeleteDate)
                VALUES 
                    (@ClientId, @AssetTypeId, @Count, @AuthorId, @CreateDate, @DeleteDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(ClientAsset entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE ClientAssets
                SET 
                    ClientId = @ClientId, 
                    AssetTypeId = @AssetTypeId,
                    Count = @Count, 
                    AuthorId = @AuthorId, 
                    CreateDate = @CreateDate, 
                    DeleteDate = @DeleteDate
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            ClientAsset entity = Get(id);
            if (entity == null)
                throw new PawnshopApplicationException($"Запись ClientAdditionalContacts с Id {id} не найдена");

            entity.DeleteDate = DateTime.Now;
            Update(entity);
        }

        public ClientAsset Get(int id)
        {
            return UnitOfWork.Session.Query<ClientAsset>(@"
                SELECT ca.*
                FROM ClientAssets ca
                WHERE ca.Id = @id AND ca.DeleteDate IS NULL",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }
        public List<ClientAsset> GetListByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<ClientAsset>(@"
                SELECT ca.*
                FROM ClientAssets ca
                WHERE ca.ClientId = @clientId AND ca.DeleteDate IS NULL",
            new { clientId }, UnitOfWork.Transaction).ToList();
        }

        public ClientAsset Find(object query)
        {
            throw new NotImplementedException();
        }
        public List<ClientAsset> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void LogChanges(ClientAsset entity, int userId, bool isNew = false)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = new ClientAssetLog
            {
                ClientAssetId = entity.Id,
                ClientId = entity.ClientId,
                AssetTypeId = entity.AssetTypeId,
                Count = entity.Count,
                AuthorId = entity.AuthorId,
                CreateDate = entity.CreateDate,
                DeleteDate = entity.DeleteDate,
                UpdatedByAuthorId = !isNew ? userId : default(int?),
                UpdateDate = DateTime.Now
            };
            UnitOfWork.Session.Execute(@"
                INSERT INTO ClientAssetLogs (ClientAssetId, ClientId, AssetTypeId, Count, AuthorId, CreateDate, DeleteDate, UpdatedByAuthorId, UpdateDate)
                VALUES (@ClientAssetId, @ClientId, @AssetTypeId, @Count, @AuthorId, @CreateDate, @DeleteDate, @UpdatedByAuthorId, @UpdateDate)", log, UnitOfWork.Transaction);
        }
    }
}
