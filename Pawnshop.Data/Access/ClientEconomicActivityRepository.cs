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
    public class ClientEconomicActivityRepository : RepositoryBase, IRepository<ClientEconomicActivity>
    {
        public ClientEconomicActivityRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientEconomicActivity entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ClientEconomicActivities(ClientId, EconomicActivityTypeId, CreateDate, AuthorId, ValueKindId)
                        VALUES(@ClientId, @EconomicActivityTypeId, @CreateDate, @AuthorId, @ValueKindId)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientEconomicActivity entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientEconomicActivities SET ClientId = @ClientId, EconomicActivityTypeId = @EconomicActivityTypeId,
                        CreateDate = @CreateDate, AuthorId = @AuthorId, ValueKindId = @ValueKindId
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientEconomicActivity Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientEconomicActivity>(@"
                SELECT * 
                    FROM ClientEconomicActivities
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ClientEconomicActivity Find(object query)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientEconomicActivities SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ClientEconomicActivity> List(ListQuery listQuery, object query = null)
        {
            var clientId = query?.Val<int?>("ClientId");

            return UnitOfWork.Session.Query<ClientEconomicActivity>($@"
                SELECT * FROM ClientEconomicActivities WHERE DeleteDate IS NULL AND ClientId = @clientId", new { clientId },
                UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var clientId = query?.Val<int?>("ClientId");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM ClientEconomicActivities WHERE DeleteDate IS NULL AND ClientId = @clientId", new { clientId },
                    UnitOfWork.Transaction);
        }
    }
}