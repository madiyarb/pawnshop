using Dapper.Contrib.Extensions;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ClientLogItems;
using Pawnshop.Data.Models.ApplicationOnlineLog.Views;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Models.ClientLogItems.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ClientLogItemsRepository : RepositoryBase
    {
        public ClientLogItemsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientLogItem logItem)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Insert(logItem, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientLogData GetLast(int clientId)
        {
            return  UnitOfWork.Session.QueryFirstOrDefault<ClientLogData>
            (@"Select top 1 * from ClientLogItems where ClientId = @clientId ORDER BY CreateDate DESC",
                new { clientId }, UnitOfWork.Transaction);
        }

        public async Task<ClientLogListItemView> GetListView(int clientId, int offset = 0, int limit = int.MaxValue)
        {
            var list = new ClientLogListItemView();
            var builder = new SqlBuilder();
            builder.Where("ClientId = @clientId", new { clientId });
            builder.Join("Users ON Users.Id = ClientLogItems.UserId");
            builder.OrderBy("CreateDate Desc");
            var selector = builder.AddTemplate(@$"SELECT ClientLogItems.*, Users.FullName AS UserName from ClientLogItems /**join**/ /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ClientLogItems /**where**/");
            return new ClientLogListItemView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                Items = (await UnitOfWork.Session.QueryAsync<ClientLogItemView>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }
    }
}
