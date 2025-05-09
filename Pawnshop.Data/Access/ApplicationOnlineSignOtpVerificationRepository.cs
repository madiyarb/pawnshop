using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System;
using System.Linq;
using Pawnshop.Data.Models.ApplicationOnlineSignOtpVerifications;
using Dapper;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineSignOtpVerificationRepository : RepositoryBase
    {
        public ApplicationOnlineSignOtpVerificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public ApplicationOnlineSignOtpVerification Get(Guid applicationId)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineSignOtpVerification>(@"SELECT TOP 1 * FROM ApplicationOnlineSignOtpVerification 
                WHERE ApplicationOnlineId = @applicationId
                ORDER BY CreateDate desc",
                new { applicationId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Insert(ApplicationOnlineSignOtpVerification verification)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"
                INSERT INTO ApplicationOnlineSignOtpVerification (Id, ApplicationOnlineId, Code, CreateDate, RetryCount, TryCount, PhoneNumber, Success, UpdateDate)
                 VALUES (@Id, @ApplicationOnlineId, @Code, @CreateDate, @RetryCount, @TryCount, @PhoneNumber, @Success, @UpdateDate)
                SELECT SCOPE_IDENTITY()", verification, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ApplicationOnlineSignOtpVerification verification)
        {
            string query = @$"UPDATE ApplicationOnlineSignOtpVerification 
                            SET TryCount={verification.TryCount}, 
                            Success='{verification.Success}', 
                            UpdateDate='{verification.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}' 
                            WHERE  Id='{verification.Id}';";
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(query, new { }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
