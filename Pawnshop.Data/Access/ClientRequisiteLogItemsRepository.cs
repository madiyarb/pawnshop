using Dapper.Contrib.Extensions;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ClientRequisiteLogItems;
using Pawnshop.Data.Models.ApplicationOnlineCarLogItems.Views;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Models.ClientRequisiteLogItems.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ClientRequisiteLogItemsRepository : RepositoryBase
    {
        public ClientRequisiteLogItemsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }
        public void Insert(ClientRequisiteLogItem logItem)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Insert(logItem, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientRequisiteLogData GetLast(int clientId, int requisiteId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ClientRequisiteLogData>
            (@"Select top 1 * from ClientRequisiteLogItems where ClientId = @clientId AND RequisiteId = @requisiteId ORDER BY CreateDate DESC",
                new { clientId, requisiteId }, UnitOfWork.Transaction);
        }

        public async Task<ClientRequisiteLogListItemView> GetListView(int clientId, int offset = 0, int limit = int.MaxValue)
        {
            var builder = new SqlBuilder();
            builder.Where("ClientId = @clientId", new { clientId });
            builder.Join("Users ON Users.Id = ClientRequisiteLogItems.UserId");
            builder.LeftJoin("RequisiteTypes ON RequisiteTypes.id = ClientRequisiteLogItems.RequisiteTypeId");
            builder.LeftJoin("Clients ON Clients.id = ClientRequisiteLogItems.BankId");
            builder.OrderBy("CreateDate");
            var selector = builder.AddTemplate(@$"SELECT ClientRequisiteLogItems.*, RequisiteTypes.Name AS RequisiteTypeName,
            Clients.FullName AS BankName, Users.FullName AS UserName from ClientRequisiteLogItems  /**leftjoin**/ /**join**/ /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ClientRequisiteLogItems /**where**/");
            return new ClientRequisiteLogListItemView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                Items = (await UnitOfWork.Session.QueryAsync<ClientRequisiteLogItemView>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }

    }
}
