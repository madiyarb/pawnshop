using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnline.Kdn;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineKdnLogRepository : RepositoryBase, IRepository<ApplicationOnlineKdnLog>
    {
        public ApplicationOnlineKdnLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineKdnLog Find(object query)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineKdnLog Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(ApplicationOnlineKdnLog entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ApplicationOnlineKdnLogs ( CreateDate, AuthorId, ClientId, ApplicationOnlineId, ApplicationOnlineStatus, ApplicationAmount, ApplicationTerm, ApplicationSettingId, TotalIncome, OtherPaymentsAmount, AverageMonthlyPayment, IncomeConfirmed, Kdn, Success, ResultText, CompareIncomeAmount, CompareExpensesAmount, IsGambler, IsStopCredit, AvgPaymentToday, TotalIncomeK4, KdnK4, AllLoan)
VALUES ( @CreateDate, @AuthorId, @ClientId, @ApplicationOnlineId, @ApplicationOnlineStatus, @ApplicationAmount, @ApplicationTerm, @ApplicationSettingId, @TotalIncome, @OtherPaymentsAmount, @AverageMonthlyPayment, @IncomeConfirmed, @Kdn, @Success, @ResultText, @CompareIncomeAmount, @CompareExpensesAmount, @IsGambler, @IsStopCredit, @AvgPaymentToday, @TotalIncomeK4, @KdnK4, @AllLoan)

SELECT SCOPE_IDENTITY()",
                entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineKdnLog> List(ListQuery listQuery, object query = null)
        {
            var appOnlineId = query.Val<Guid?>("ApplicationOnlineId");

            if (!appOnlineId.HasValue)
                return new List<ApplicationOnlineKdnLog>();

            return UnitOfWork.Session.Query<ApplicationOnlineKdnLog, User, ApplicationOnlineKdnLog>(@"SELECT l.*,
       u.*
  FROM ApplicationOnlineKdnLogs l
  JOIN Users u ON u.Id = l.AuthorId
 WHERE l.DeleteDate IS NULL
   AND l.ApplicationOnlineId = @appOnlineId",
                (l, u) =>
                {
                    l.Author = u;
                    return l;
                },
                new { appOnlineId }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(ApplicationOnlineKdnLog entity)
        {
            throw new NotImplementedException();
        }
    }
}
