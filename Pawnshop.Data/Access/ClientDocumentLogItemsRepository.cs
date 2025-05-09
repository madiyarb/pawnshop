using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Data.Models.ClientDocumentLogItems;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.ClientDocumentLogItems.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ClientDocumentLogItemsRepository : RepositoryBase
    {
        public ClientDocumentLogItemsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientDocumentLogItem logItem)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Insert(logItem, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientDocumentLogData GetLast(int clientId, int documentId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ClientDocumentLogData>
            (@"Select top 1 * from ClientDocumentLogItems where ClientId = @clientId AND DocumentId = @documentId  ORDER BY CreateDate DESC",
                new { clientId, documentId }, UnitOfWork.Transaction);
        }

        public async Task<ClientDocumentLogListItemView> GetListView(int clientId, int offset = 0, int limit = int.MaxValue)
        {
            var list = new ClientDocumentLogListItemView();
            var builder = new SqlBuilder();
            builder.Where("ClientId = @clientId", new { clientId });
            builder.Join("Users ON Users.Id = ClientDocumentLogItems.UserId");
            builder.Join("ClientDocumentProviders ON ClientDocumentProviders.Id = ClientDocumentLogItems.ProviderId");
            builder.OrderBy("CreateDate");
            var selector = builder.AddTemplate(@$"SELECT ClientDocumentLogItems.*, Users.FullName AS UserName from ClientDocumentLogItems /**join**/ /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ClientDocumentLogItems /**where**/");
            return new ClientDocumentLogListItemView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                Items = (await UnitOfWork.Session.QueryAsync<ClientDocumentLogItemView>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }
    }
}
