using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using System;
using System.Linq;
using Pawnshop.Data.Models.TasLabRecruit;
using Pawnshop.Core.Queries;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class TasLabRecruitRequestsRepository : RepositoryBase
    {
        public TasLabRecruitRequestsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public RecruitRequest Find(ListQuery listQuery, object query)
        {
            var predicate = new StringBuilder();
            predicate.Append("WHERE DeleteDate IS NULL");
            var order = listQuery.Order("", listQuery.Sort);

            var id = query?.Val<int?>("Id");
            var requestType = query?.Val<int?>("RequestType");
            var userId = query?.Val<int?>("UserId");
            var requestIndex = query?.Val<long?>("RequestIndex");
            var requestIIN = query?.Val<string>("RequestIIN");
            var responseIndex = query?.Val<int?>("ResponseIndex");
            var errorMessage = query?.Val<string>("ErrorMessage");
            var createDate = query?.Val<DateTime?>("CreateDate");

            predicate.Append(id.HasValue ? " AND Id = @id" : string.Empty);
            predicate.Append(requestType.HasValue ? " AND RequestType = @requestType" : string.Empty);
            predicate.Append(userId.HasValue ? " AND UserId = @userId" : string.Empty);
            predicate.Append(requestIndex.HasValue ? " AND RequestIndex = @requestIndex" : string.Empty);
            predicate.Append(!string.IsNullOrEmpty(requestIIN) ? " AND RequestIIN = @requestIIN" : string.Empty);
            predicate.Append(responseIndex.HasValue ? " AND ResponseIndex = @responseIndex" : string.Empty);
            predicate.Append(!string.IsNullOrEmpty(errorMessage) ? " AND ErrorMessage = @errorMessage" : string.Empty);
            predicate.Append(createDate.HasValue ? " AND CreateDate = @createDate" : string.Empty);
            predicate.Append(createDate.HasValue ? " AND CBType = @cBType" : string.Empty);

            return UnitOfWork.Session.Query<RecruitRequest>(@$"
                SELECT * 
                FROM TasLabRecruitRequests
                    {predicate}
                    {order}
            ", new { 
                id, 
                requestType, 
                userId, 
                requestIndex, 
                requestIIN, 
                responseIndex,
                errorMessage, 
                createDate 
            }).FirstOrDefault();
        }
        
        public void Insert(RecruitRequest entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO TasLabRecruitRequests (RequestType, UserId, RequestIndex, RequestIIN, ResponseIndex, ErrorMessage, CreateDate, CBType, ResponseJson)
                    VALUES (@RequestType, @UserId, @RequestIndex, @RequestIIN, @ResponseIndex, @ErrorMessage, @CreateDate, @CBType, @ResponseJson)
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}