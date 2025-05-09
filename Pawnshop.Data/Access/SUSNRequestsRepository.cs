using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.SUSNRequests;

namespace Pawnshop.Data.Access
{
    public sealed class SUSNRequestsRepository : RepositoryBase
    {

        public SUSNRequestsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public async Task<SUSNRequest> GetLastRequestByClientId(int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("SUSNRequests.*");
            builder.Where("SUSNRequests.Successfully = 1");
            builder.Where("SUSNRequests.ClientId = @clientId", new {clientId});
            builder.OrderBy("SUSNRequests.CreateDate DESC");
            var selector = builder.AddTemplate($"Select TOP 1 /**select**/ from SUSNRequests /**where**/ /**orderby**/ ");
            return (await UnitOfWork.Session.QueryAsync<SUSNRequest>(selector.RawSql,
                selector.Parameters)).FirstOrDefault();
        }

        public async Task Insert(SUSNRequest request)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(request, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

    }
}
