using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.ClientAddressLogItems;
using Pawnshop.Data.Models.ClientLogItems.Views;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.ClientAddressLogItems.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ClientAddressLogItemsRepository : RepositoryBase
    {
        public ClientAddressLogItemsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientAddressLogItem logItem)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Insert(logItem, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientAddressLogData GetLast(int clientId, int addressId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ClientAddressLogData>
            (@"Select top 1 * from ClientAddressLogItems where ClientId = @clientId and AddressId = addressId ORDER BY CreateDate DESC",
                new { clientId, addressId }, UnitOfWork.Transaction);
        }

        public async Task<ClientAddressLogItemListView> GetListView(int clientId, int offset = 0, int limit = int.MaxValue)
        {
            var builder = new SqlBuilder();
            builder.Where("ClientId = @clientId", new { clientId });
            builder.Join("Users ON Users.Id = ClientAddressLogItems.UserId");
            builder.Join("AddressTypes ON AddressTypes.Id = ClientAddressLogItems.AddressTypeId");
            builder.Join("Countries ON Countries.Id = ClientAddressLogItems.CountryId");
            builder.OrderBy("CreateDate Desc");
            var selector = builder.AddTemplate(@$"SELECT ClientAddressLogItems.*, Countries.NameRus AS CountryName, 
AddressTypes.Name AS AddressTypeName, Users.FullName AS UserName from ClientAddressLogItems /**join**/ /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ClientAddressLogItems /**where**/");
            return new ClientAddressLogItemListView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                Items = (await UnitOfWork.Session.QueryAsync<ClientAddressLogItemView>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }
    }
}
