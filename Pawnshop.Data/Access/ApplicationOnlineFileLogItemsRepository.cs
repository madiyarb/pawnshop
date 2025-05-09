using Dapper.Contrib.Extensions;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Models.ApplicationOnlineFileLogItems;
using System.Linq;
using Pawnshop.Data.Models.ApplicationOnlineFileLogItems.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineFileLogItemsRepository : RepositoryBase
    {
        public ApplicationOnlineFileLogItemsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        public async Task Insert(ApplicationOnlineFileLogItem logItem)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(logItem, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<ApplicationOnlineFileLogData> GetLast(Guid fileId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ApplicationOnlineFileLogData>
            (@"Select top 1 * from ApplicationOnlineFileLogItems where FileId = @fileId ORDER BY CreateDate DESC",
                new { fileId }, UnitOfWork.Transaction);
        }

        public async Task<ApplicationOnlineFileLogItemListView> GetListView(Guid applicationId, int offset = 0, int limit = int.MaxValue)
        {
            var builder = new SqlBuilder();
            builder.Where("ApplicationId = @applicationId", new { applicationId });
            builder.Join("Users ON Users.Id = ApplicationOnlineFileLogItems.UserId");
            builder.Join("ApplicationOnlineFileCodes ON ApplicationOnlineFileCodes.Id = ApplicationOnlineFileLogItems.ApplicationOnlineFileCode");
            builder.OrderBy("CreateDate");
            var selector = builder.AddTemplate(@$"SELECT ApplicationOnlineFileLogItems.*, ApplicationOnlineFileCodes.Title, Users.FullName AS UserName from ApplicationOnlineFileLogItems /**join**/ /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ApplicationOnlineFileLogItems /**where**/");
            return new ApplicationOnlineFileLogItemListView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                Items = (await UnitOfWork.Session.QueryAsync<ApplicationOnlineFileLogItemView>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }
    }
}
