using System;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineCarLogItems;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.ApplicationOnlineCarLogItems.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineCarLogItemRepository : RepositoryBase
    {
        public ApplicationOnlineCarLogItemRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ApplicationOnlineCarLogItem logItem)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Insert(logItem, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ApplicationOnlineCarLogData GetLast(Guid applicationOnlineCarId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlineCarLogData>
            (@"Select top 1 * from ApplicationOnlineCarLogItems where ApplicationOnlineCarId = @applicationOnlineCarId ORDER BY CreateDate DESC",
                new { applicationOnlineCarId }, UnitOfWork.Transaction);
        }

        public async Task<ApplicationOnlineCarLogListItemView> GetListView(Guid applicationOnlineCarId, int offset = 0, int limit = int.MaxValue)
        {
            var builder = new SqlBuilder();
            builder.Where("ApplicationOnlineCarId = @applicationOnlineCarId", new { applicationOnlineCarId });
            builder.Join("Users ON Users.Id = ApplicationOnlineCarLogItems.UserId");
            builder.LeftJoin("VehicleMarks ON VehicleMarks.id = ApplicationOnlineCarLogItems.VehicleMarkId");
            builder.LeftJoin("VehicleModels ON VehicleModels.id = ApplicationOnlineCarLogItems.VehicleModelId");
            builder.OrderBy("CreateDate");
            var selector = builder.AddTemplate(@$"SELECT ApplicationOnlineCarLogItems.*, VehicleModels.Name AS VehicleModelName,
            VehicleMarks.Name AS VehicleMarkName, Users.FullName AS UserName from ApplicationOnlineCarLogItems  /**leftjoin**/ /**join**/ /**where**/ /**orderby**/ 
            OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var countTemplate = builder.AddTemplate(
                @$"SELECT COUNT(*) from ApplicationOnlineCarLogItems /**where**/");
            return new ApplicationOnlineCarLogListItemView
            {
                Count = (await UnitOfWork.Session.QueryAsync<int>(countTemplate.RawSql, selector.Parameters))
                    .FirstOrDefault(),
                Items = (await UnitOfWork.Session.QueryAsync<ApplicationOnlineCarLogItemView>(selector.RawSql,
                    selector.Parameters)).ToList()
            };
        }
    }
}
