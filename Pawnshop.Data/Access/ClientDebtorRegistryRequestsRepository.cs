using System.Collections.Generic;
using System.Linq;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ClientDebtorRegistryRequests;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Data.Models.SUSNRequests;

namespace Pawnshop.Data.Access
{
    public sealed class ClientDebtorRegistryRequestsRepository : RepositoryBase
    {
        public ClientDebtorRegistryRequestsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public async Task Insert(ClientDebtorRegistryRequest request)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(request, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<ClientDebtorRegistryRequest> GetLastRequest(int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientDebtorRegistryRequests.*");
            builder.Where("ClientDebtorRegistryRequests.ClientId = @clientId", new { clientId });
            builder.OrderBy("ClientDebtorRegistryRequests.CreateDate DESC");
            var selector = builder.AddTemplate($"Select TOP 1 /**select**/ from ClientDebtorRegistryRequests /**where**/ /**orderby**/ ");
            return (await UnitOfWork.Session.QueryAsync<ClientDebtorRegistryRequest>(selector.RawSql,
                selector.Parameters)).FirstOrDefault();
        }
    }
}
