using Dapper.Contrib.Extensions;
using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.OnlineTasks.Views;
using Pawnshop.Data.Models.OnlineTasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Data.Access
{
    public sealed class OnlineTasksRepository : RepositoryBase
    {
        public OnlineTasksRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public OnlineTask Get(Guid id)
        {
            return UnitOfWork.Session.Query<OnlineTask>(@"Select * from OnlineTasks where id = @id",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public OnlineTask GetByApplicationId(Guid applicationId, List<OnlineTaskStatus> statuses = null, string type = null)
        {
            var query = @$"Select top 1 * from OnlineTasks where ApplicationId = '{applicationId}' ";

            if (statuses != null)
            {
                string statusesStr = "";
                for (int i = 0; i < statuses.Count; i++)
                {
                    if (i == 0)
                    {
                        statusesStr += $@"'{statuses[i].ToString()}'";
                    }
                    else
                    {
                        statusesStr += $@",'{statuses[i].ToString()}'";
                    }
                }

                query += $@"and status in ({statusesStr}) ";
            }

            if (!string.IsNullOrEmpty(type))
            {
                query += $@"and type = '{type}'";
            }

            query += " Order by CreateDate desc";

            return UnitOfWork.Session.Query<OnlineTask>(query,
                new { }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<IEnumerable<OnlineTask>> GetOnlineTasksByApplicationId(Guid applicationId,
            List<OnlineTaskStatus> statuses = null, string type = null)
        {
            var builder = new SqlBuilder();
            builder.Select("OnlineTasks.*");
            builder.Where("OnlineTasks.ApplicationId = @applicationId", new { applicationId = applicationId.ToString() });
            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    builder.OrWhere("OnlineTasks.status = @status", new { status = status.ToString() });
                }
            }

            if (type != null)
                builder.Where("OnlineTasks.type = @type", type);
            var selector = builder.AddTemplate($"Select /**select**/ from OnlineTasks /**where**/");
            return await UnitOfWork.Session.QueryAsync<OnlineTask>(selector.RawSql,
                selector.Parameters);
        }

        public OnlineTaskView GetView(Guid id)
        {
            var builder = new SqlBuilder();
            builder.Select("OnlineTasks.*");
            builder.Select("OnlineTasks.Type AS TypeCode");
            builder.Select("CreationUser.FullName AS CreationAuthor");
            builder.Select("WorkerUser.FullName AS Worker");
            builder.Select("Clients.FullName");
            builder.Select("ApplicationsOnline.ApplicationNumber");
            builder.Select("Leads.Name AS LeadName");
            builder.Select("Leads.Patronymic AS LeadPatronymic");
            builder.Select("Leads.Surname AS LeadSurname");
            builder.Select("Leads.Phone AS LeadPhone");

            builder.LeftJoin("Users AS CreationUser  ON CreationUser.Id = OnlineTasks.CreationUserId");
            builder.LeftJoin("Users AS WorkerUser ON WorkerUser.Id = OnlineTasks.UserId");
            builder.LeftJoin("Clients ON Clients.Id = OnlineTasks.ClientId");
            builder.LeftJoin("ApplicationsOnline ON ApplicationsOnline.Id = OnlineTasks.ApplicationId");
            builder.LeftJoin("Leads on Leads.Id = OnlineTasks.LeadId");


            builder.Where("OnlineTasks.Id = @id", new { id });

            var selector = builder.AddTemplate($"Select /**select**/ from OnlineTasks /**leftjoin**/ /**where**/");
            return UnitOfWork.Session.QueryFirstOrDefault<OnlineTaskView>(selector.RawSql,
                selector.Parameters);
        }

        public void Insert(OnlineTask task)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Insert(task, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(OnlineTask task)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Update(task, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<OnlineTaskListView> GetListView(int offset, int limit, DateTime startDate, DateTime endDate, int? userId, string partnerCode, int? minutesLeftAfterUpdate = null, int? branchId = null)
        {
            var builder = new SqlBuilder();
            builder.Select("OnlineTasks.*");
            builder.Select("OnlineTasks.Type AS TypeCode");
            builder.Select("CreationUser.FullName AS CreationAuthor");
            builder.Select("WorkerUser.FullName AS Worker");
            builder.Select("Clients.FullName AS ClientName");
            builder.Select("ApplicationsOnline.ApplicationNumber");
            builder.Select("ApplicationsOnline.ApplicationAmount");
            builder.Select("Leads.Name AS LeadName");
            builder.Select("Leads.Patronymic AS LeadPatronymic");
            builder.Select("Leads.Surname AS LeadSurname");
            builder.Select("Leads.Phone AS LeadPhone");
            builder.Select("Clients.PartnerCode AS PartnerCode");

            builder.LeftJoin("Users AS CreationUser  ON CreationUser.Id = OnlineTasks.CreationUserId");
            builder.LeftJoin("Users AS WorkerUser ON WorkerUser.Id = OnlineTasks.UserId");
            builder.LeftJoin("Clients ON Clients.Id = OnlineTasks.ClientId");
            builder.LeftJoin("ApplicationsOnline ON ApplicationsOnline.Id = OnlineTasks.ApplicationId");
            builder.LeftJoin("Leads on Leads.Id = OnlineTasks.LeadId");
            builder.LeftJoin("BranchesPartnerCodes ON BranchesPartnerCodes.PartnerCode = Clients.PartnerCode");

            builder.Where($"OnlineTasks.CreateDate > '{startDate:yyyy-MM-dd HH:mm:ss.fff}'");
            builder.Where($"OnlineTasks.CreateDate < '{endDate:yyyy-MM-dd HH:mm:ss.fff}'");

            if (!string.IsNullOrEmpty(partnerCode))
            {
                builder.Where($"Clients.PartnerCode = '{partnerCode}'");
            }

            builder.OrderBy("CreateDate DESC");

            if (branchId.HasValue)
            {
                builder.Where(@"((Clients.PartnerCode IS NULL 
   OR BranchesPartnerCodes.Id IS NULL 
   OR DATEADD(MINUTE, 10, OnlineTasks.CreateDate) < dbo.GETASTANADATE() 
   OR BranchesPartnerCodes.BranchId = @BranchId)
OR ApplicationsOnline.Status IN ('Verification', 'RequisiteCheck', 'BiometricPassed'))", new { BranchId = branchId.Value });
            }

            if (minutesLeftAfterUpdate.HasValue)
            {
                builder.Where(
                    $"DATEDIFF(MINUTE, OnlineTasks.CreateDate, dbo.GETASTANADATE()) >= {minutesLeftAfterUpdate}");
            }

            if (userId.HasValue)
            {
                builder.Where(
                    $"(OnlineTasks.Status = 'Created' OR (OnlineTasks.Status = 'Processing' AND OnlineTasks.UserId = {userId}))");// Незнаю зачем так но оставил
            }
            else
            {
                builder.Where("OnlineTasks.Status = 'Created'");// Незнаю зачем так но оставил
            }
            var selector = builder.AddTemplate($"Select /**select**/ from OnlineTasks /**leftjoin**/ /**where**/ /**orderby**/ OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var count = builder.AddTemplate("SELECT COUNT(*) from OnlineTasks /**leftjoin**/ /**where**/");

            OnlineTaskListView listView = new OnlineTaskListView();

            listView.Count = await UnitOfWork.Session.QueryFirstAsync<int>(count.RawSql, selector.Parameters);

            listView.Tasks = await UnitOfWork.Session.QueryAsync<OnlineTaskView>(selector.RawSql, selector.Parameters);

            listView.FillData();

            return listView;

        }

        public List<OnlineTask> GetAllCallbackNotCompletedTasksByMissingCalls(string phoneNumber)
        {
            var builder = new SqlBuilder();
            builder.Select("OnlineTasks.*");
            builder.Join(
                "Calls ON OnlineTasks.CallId = Calls.id");
            builder.Where("OnlineTasks.Type = 'CallBack'");
            builder.Where("OnlineTasks.Status != 'Completed'");
            builder.Where("Calls.Status = 'NOANSWER'");
            builder.Where("Calls.Direction = 'incoming'");
            builder.Where("Calls.PhoneNumber = @phoneNumber", new { phoneNumber });
            builder.OrderBy("UpdateDate");
            var selector = builder.AddTemplate($"Select /**select**/ from OnlineTasks /**join**/ /**where**/");
            return UnitOfWork.Session.Query<OnlineTask>(selector.RawSql,
                selector.Parameters).ToList();
        }

        public List<OnlineTask> GetAllNotCompletedCallBackTasksByLead(string phoneNumber)
        {
            var builder = new SqlBuilder();
            builder.Select("OnlineTasks.*");
            builder.Join(
                "Leads ON OnlineTasks.LeadId = Leads.id");
            builder.Where("OnlineTasks.Type = 'CallBack'");
            builder.Where("OnlineTasks.Status != 'Completed'");
            builder.Where("Leads.Phone = @phoneNumber", new { phoneNumber });
            builder.OrderBy("UpdateDate");
            var selector = builder.AddTemplate($"Select /**select**/ from OnlineTasks /**join**/ /**where**/");
            return UnitOfWork.Session.Query<OnlineTask>(selector.RawSql,
                selector.Parameters).ToList();
        }


    }
}
