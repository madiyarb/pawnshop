using System;
using System.Linq;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Data.Models.ApplicationOnlineLog;
using Dapper.Contrib.Extensions;
using Pawnshop.Data.Models.ApplicationOnlineLog.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineLogItemsRepository : RepositoryBase
    {
        public ApplicationOnlineLogItemsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task Insert(ApplicationOnlineLogItem logItem)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(logItem, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<ApplicationOnlineLogData> GetLast(Guid applicationId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ApplicationOnlineLogData>
            (@"Select top 1 * from ApplicationOnlineLogItems where ApplicationId = @applicationId ORDER BY CreateDate DESC",
                new { applicationId }, UnitOfWork.Transaction);
        }

        public async Task<ApplicationOnlineLogItemListView> GetListView(Guid applicationId, int offset = 0, int limit = int.MaxValue)
        {
            var list = new ApplicationOnlineLogItemListView();
            var builder = new SqlBuilder();
            builder.Select("ApplicationOnlineLogItems.*");
            builder.Select("LoanPercentSettings.Name AS ProductName");
            builder.Select("Users.FullName AS UserName");
            builder.Where("ApplicationId = @applicationId", new { applicationId });
            builder.Join("Users ON Users.Id = ApplicationOnlineLogItems.UserId");
            builder.Join("LoanPercentSettings ON LoanPercentSettings.Id = ApplicationOnlineLogItems.ProductId");
            builder.OrderBy("CreateDate");
            var selector = builder.AddTemplate(@$"SELECT /**select**/ from ApplicationOnlineLogItems /**join**/ /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ApplicationOnlineLogItems /**where**/");
            return new ApplicationOnlineLogItemListView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                Items = (await UnitOfWork.Session.QueryAsync<ApplicationOnlineLogItemView>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }

    }
}
