using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.JetPay;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class JetPayCardPayoutInformationRepository : RepositoryBase
    {
        public JetPayCardPayoutInformationRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task DeleteAsync(int id)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"UPDATE JetPayCardPayoutInformations
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task<JetPayCardPayoutInformation> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<JetPayCardPayoutInformation>(@"SELECT *
  FROM JetPayCardPayoutInformations
 WHERE Id = @id
   AND DeleteDate IS NULL",
                new { id }, UnitOfWork.Transaction);
        }

        public async Task<JetPayCardPayoutInformation> GetByTokenAsync(string token)
        {
            var result = await UnitOfWork.Session.QueryAsync<JetPayCardPayoutInformation, ClientRequisite, JetPayCardPayoutInformation>(@"SELECT TOP 1 jp.*,
       clr.*
  FROM JetPayCardPayoutInformations jp
  JOIN ClientRequisites clr ON clr.Id = jp.ClientRequisiteId
 WHERE jp.Token = @token
   AND jp.DeleteDate IS NULL",
                (jp, clr) =>
                {
                    jp.ClientRequisite = clr;
                    return jp;
                },
                new { token }, UnitOfWork.Transaction);

            return result.FirstOrDefault();
        }

        public async Task InsertAsync(JetPayCardPayoutInformation entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = await UnitOfWork.Session.QuerySingleOrDefaultAsync<int>(@"
INSERT INTO JetPayCardPayoutInformations ( CreateDate, ClientRequisiteId, Token, CustomerIp, CustomerId )
VALUES ( @CreateDate, @ClientRequisiteId, @Token, @CustomerIp, @CustomerId )

SELECT SCOPE_IDENTITY()",
                entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task UpdateAsync(JetPayCardPayoutInformation entity)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"UPDATE JetPayCardPayoutInformations
   SET ClientRequisiteId = @ClientRequisiteId,
       Token = @Token,
       CustomerIp = @CustomerIp,
       CustomerId = @CustomerId
 WHERE Id = @id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
