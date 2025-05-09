using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ApplicationOnlineStatusChangeHistories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.ApplicationOnlineStatusChangeHistories.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineStatusChangeHistoryRepository : RepositoryBase
    {
        public ApplicationOnlineStatusChangeHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public async Task Insert(ApplicationOnlineStatusChangeHistory applicationOnline)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.QuerySingleAsync(@"
                INSERT INTO ApplicationOnlineStatusChangeHistories (Id, ApplicationOnlineId, UserId, UserRole, CreateDate, ApplicationAmount, EstimatedCost, Stage, Status, Decision, DeclineReason) 
                VALUES (@Id, @ApplicationOnlineId, @UserId, @UserRole, @CreateDate, @ApplicationAmount, @EstimatedCost, @Stage, @Status, @Decision, @DeclineReason) ;
                SELECT SCOPE_IDENTITY()", applicationOnline, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ApplicationOnlineStatusChangeHistory Get(int id)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineStatusChangeHistory>(@"Select * from ApplicationOnlineStatusChangeHistories where id = @id",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }


        public ApplicationOnlineStatusChangeHistoryListView GetListView(Guid applicationId, int offset, int limit)
        {
            string query = $@"SELECT ApplicationOnlineStatusChangeHistories.*, Users.FullName AS UserName from ApplicationOnlineStatusChangeHistories
                    LEFT JOIN Users ON ApplicationOnlineStatusChangeHistories.UserId = Users.Id
                    WHERE ApplicationOnlineId = '{applicationId}'";

            string countquery = $@"SELECT COUNT(*)
            FROM ApplicationOnlineStatusChangeHistories 
            WHERE ApplicationOnlineId = '{applicationId}'";
            string tail = @$" ORDER BY CreateDate 
                OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY";
            string result = query + tail;

            ApplicationOnlineStatusChangeHistoryListView listView = new ApplicationOnlineStatusChangeHistoryListView();
            listView.Count = UnitOfWork.Session.Query<int>(countquery,
                new { }, UnitOfWork.Transaction).FirstOrDefault();

            if (listView.Count == 0)
                return null;
            listView.ApplicationOnlineStatusChangeHistories = UnitOfWork.Session.Query<ApplicationOnlineStatusChangeHistoryView>(result,
                new { }, UnitOfWork.Transaction).ToList();
            listView.FillStatuses();
            return listView;

        }
    }
}
