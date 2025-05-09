using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Data.Models.UKassa;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class UKassaRepository : RepositoryBase, IRepository<UKassaRequest>
    {
        public UKassaRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public UKassaRequest Find(object query)
        {
            throw new NotImplementedException();
        }

        public UKassaRequest Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<UKassaRequest>(@"SELECT * FROM UKassaRequests WHERE Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public UKassaRequest GetByOrderId(int orderId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<UKassaRequest>(@"SELECT * FROM UKassaRequests WHERE CashOrderId = @orderId",
                new { orderId }, UnitOfWork.Transaction);
        }

        public List<UKassaRequest> GetNewRequests(List<int> orders)
        {
            return UnitOfWork.Session.Query<UKassaRequest>(@";
            UPDATE UKassaRequests SET Status = 11
            OUTPUT Inserted.*
            WHERE Status = 10 AND CashOrderId IN @orders",
                new { orders = orders }, UnitOfWork.Transaction).ToList();
        }

        public void Insert(UKassaRequest entity)
        {
            entity.Id = UnitOfWork.Session.Execute(@"
            INSERT INTO UKassaRequests ( RequestId, CashOrderId, RequestData, ResponseData, ResponseCheckNumber, AuthorId, KassaId, SectionId, ShiftNumber, TotalAmount, OperationType, CreateDate, DeleteDate, Status, RequestUrl, ResponseDate )
            VALUES ( @RequestId, @CashOrderId, @RequestData, null, null, @AuthorId, @KassaId, @SectionId, @ShiftNumber, @TotalAmount, @OperationType, dbo.GETASTANADATE(), null, @Status, @RequestUrl, null );",
                entity, UnitOfWork.Transaction);
        }

        public List<UKassaRequest> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(UKassaRequest entity)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaRequests
                    SET ResponseData = @ResponseData, Status = @Status, ShiftNumber = @ShiftNumber, 
                        ResponseCheckNumber = @ResponseCheckNumber, ResponseDate = @ResponseDate, RequestDate = @RequestDate
                        WHERE Id = @Id",
                entity, UnitOfWork.Transaction);
        }

        public void UpdateStatus(int requestId, TasOnlineRequestStatus status)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaRequests
                    SET Status = @status
                        WHERE Id = @requestId",
                new { requestId, status }, UnitOfWork.Transaction);
        }

        public int GetKassaByBranch(int branchId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<int>($@"SELECT KassaId FROM UKassaAccountSettings WHERE AccountId = (SELECT Id FROM Accounts WHERE BranchId = {branchId} AND Code = '1010')",
                null, UnitOfWork.Transaction);
        }

        public void ReturnToNewState(List<int> ids)
        {
            var inn = "";
            if (ids.Count > 0)
            {
                inn = $" AND Id IN ({string.Join(", ", ids)})";
            }
            UnitOfWork.Session.Query($@"UPDATE UKassaRequests SET Status = 10 WHERE Status = 11 {inn};",
                null, UnitOfWork.Transaction);
        }
    }
}
