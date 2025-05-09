using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ClientExternalValidationData;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public sealed class ClientExternalValidationDataRepository : RepositoryBase
    {
        public ClientExternalValidationDataRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<ClientExternalValidationData> GetByClientId(int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientExternalValidationData.*");
            builder.Where("ClientExternalValidationData.ClientId = @clientId",
                new { clientId });
            builder.OrderBy("ClientExternalValidationData.ValidationDate DESC");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientExternalValidationData /**where**/ /**orderby**/");
            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<ClientExternalValidationData>
                    (builderTemplate.RawSql, builderTemplate.Parameters);
        }

        public async Task Insert(ClientExternalValidationData data)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(data, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

    }
}
