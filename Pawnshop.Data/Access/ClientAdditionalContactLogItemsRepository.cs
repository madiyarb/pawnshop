using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.ClientAdditionalContactLogItems;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.ClientAdditionalContactLogItems.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ClientAdditionalContactLogItemsRepository : RepositoryBase
    {
        public ClientAdditionalContactLogItemsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientAdditionalContactLogItem logItem)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Insert(logItem, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientAdditionalContactLogData GetLast(int clientId, int clientAdditionalContactId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ClientAdditionalContactLogData>
            (@"Select top 1 * from ClientAdditionalContactLogItems where ClientId = @clientId AND ClientAdditionalContactId = @clientAdditionalContactId ORDER BY CreateDate DESC",
                new { clientId, clientAdditionalContactId }, UnitOfWork.Transaction);
        }
        public async Task<ClientAdditionalContactLogItemListView> GetListView(int clientId, int offset = 0, int limit = int.MaxValue)
        {
            var builder = new SqlBuilder();
            builder.Where("ClientId = @clientId", new { clientId });
            builder.Join("Users ON Users.Id = ClientAdditionalContactLogItems.UserId");
            builder.Join("DomainValues ON DomainValues.Id = ClientAdditionalContactLogItems.ContactOwnerTypeId");
            builder.OrderBy("CreateDate Desc");
            var selector = builder.AddTemplate(@$"SELECT ClientAdditionalContactLogItems.*, DomainValues.Name AS ContactOwnerTypeName, 
            Users.FullName AS UserName from ClientAdditionalContactLogItems /**join**/ /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ClientAdditionalContactLogItems /**where**/");
            return new ClientAdditionalContactLogItemListView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                Items = (await UnitOfWork.Session.QueryAsync<ClientAdditionalContactLogItemView>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }
    }
}
