using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ClientDebtorRegistryData;

namespace Pawnshop.Data.Access
{
    public sealed class ClientDebtorRegistryDataRepository : RepositoryBase
    {
        public ClientDebtorRegistryDataRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public async Task Insert(ClientDebtorRegistryData data)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(data, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<IEnumerable<ClientDebtorRegistryData>> GetAllDataByRequestId(Guid requestId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientDebtorRegistryData.*");
            builder.Where("ClientDebtorRegistryData.ClientDebtorRegistryRequestId = @ClientDebtorRegistryRequestId", new { ClientDebtorRegistryRequestId  = requestId});
            var selector = builder.AddTemplate($"Select /**select**/ from ClientDebtorRegistryData /**where**/");
            return (await UnitOfWork.Session.QueryAsync<ClientDebtorRegistryData>(selector.RawSql,
                selector.Parameters));
        }
    }
}
