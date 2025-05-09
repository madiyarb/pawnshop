using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation.Views;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationsOnlineEstimationRepository : RepositoryBase
    {
        public ApplicationsOnlineEstimationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public ApplicationsOnlineEstimation Get(Guid id)
        {
            return UnitOfWork.Session.Query<ApplicationsOnlineEstimation>(@"Select * from ApplicationOnlineEstimations where id = @id",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ApplicationsOnlineEstimation GetLastByApplicationId(Guid applicationId)
        {
            return UnitOfWork.Session.Query<ApplicationsOnlineEstimation>(@"SELECT TOP 1 * FROM ApplicationOnlineEstimations
                WHERE ApplicationOnlineId = @applicationId ORDER BY UpdateDate DESC",
                new { applicationId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<ApplicationsOnlineEstimation> GetLastByApplicationIdAsync(Guid applicationId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ApplicationsOnlineEstimation>(@"SELECT TOP 1 * FROM ApplicationOnlineEstimations
                WHERE ApplicationOnlineId = @applicationId ORDER BY UpdateDate DESC",
                new { applicationId }, UnitOfWork.Transaction);
        }

        public ApplicationsOnlineEstimationListView GetListView(Guid applicationId, int offset, int limit)
        {
            string query = $@"SELECT * from ApplicationOnlineEstimations
                    WHERE ApplicationOnlineId = '{applicationId}'";

            string countquery = $@"SELECT COUNT(*)
            FROM ApplicationOnlineEstimations
            WHERE ApplicationOnlineId = '{applicationId}'";
            string tail = @$" ORDER BY UpdateDate 
                OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY";
            string result = query + tail;

            ApplicationsOnlineEstimationListView listView = new ApplicationsOnlineEstimationListView();
            listView.Count = UnitOfWork.Session.Query<int>(countquery,
                new { }, UnitOfWork.Transaction).FirstOrDefault();

            if (listView.Count == 0)
                return null;
            listView.Estimations = UnitOfWork.Session.Query<ApplicationsOnlineEstimationView>(result,
                new { }, UnitOfWork.Transaction).ToList();
            listView.FillStatuses();
            return listView;

        }

        public void Insert(ApplicationsOnlineEstimation applicationsOnlineEstimation)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"
                INSERT INTO ApplicationOnlineEstimations (Id, ApplicationOnlineId, Status, CreateDate, UpdateDate, EstimationServiceСlientId, EstimationServicePledgeId, EstimationServiceApplyId) 
                VALUES (@Id, @ApplicationOnlineId, @Status, @CreateDate, @UpdateDate, @EstimationServiceСlientId, @EstimationServicePledgeId, @EstimationServiceApplyId) 
                SELECT SCOPE_IDENTITY()", applicationsOnlineEstimation, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ApplicationsOnlineEstimation entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                UPDATE ApplicationOnlineEstimations
                   SET Status = @Status,
                       EvaluatedAmount = @EvaluatedAmount,
                       IssuedAmount = @IssuedAmount,
                       ValuerName = @ValuerName
                 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
