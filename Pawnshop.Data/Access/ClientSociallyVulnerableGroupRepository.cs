using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ClientSociallyVulnerableGroupRepository : RepositoryBase, IRepository<ClientSociallyVulnerableGroup>
    {
        public ClientSociallyVulnerableGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientSociallyVulnerableGroup entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ClientSociallyVulnerableGroups ( ClientId, SociallyVulnerableGroupId, BeginDate, UserId, CreateDate ) 
                        VALUES ( ClientId, SociallyVulnerableGroupId, BeginDate, UserId, CreateDate )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ClientSociallyVulnerableGroup entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientSociallyVulnerableGroups 
                        SET ClientId=@ClientId, SociallyVulnerableGroupId=@SociallyVulnerableGroupId, BeginDate=BeginDate, EndDate=@EndDate, UserId=@UserId 
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE ClientSociallyVulnerableGroups SET DeleteDate=dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientSociallyVulnerableGroup Find(object query)
        {
            throw new NotImplementedException();
        }

        public ClientSociallyVulnerableGroup Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<ClientSociallyVulnerableGroup> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public List<ClientSociallyVulnerableGroup> FindByClientId(int ClientId)
        {
            return UnitOfWork.Session.Query<ClientSociallyVulnerableGroup>($@"
                SELECT * FROM ClientSociallyVulnerableGroups 
                WHERE ClientId = @ClientId", new { ClientId }, UnitOfWork.Transaction).ToList();
        }
    }
}
