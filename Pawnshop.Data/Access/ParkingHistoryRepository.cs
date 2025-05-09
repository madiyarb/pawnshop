using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Parking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class ParkingHistoryRepository : RepositoryBase, IRepository<ParkingHistory>
    {
        public ParkingHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ParkingHistory entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ParkingHistories (ContractId, PositionId, StatusBeforeId, StatusAfterId, UserId, DelayDays, CreateDate, Note, Date, ParkingActionId, ActionId) 
                                          VALUES (@ContractId, @PositionId, @StatusBeforeId, @StatusAfterId, @UserId, @DelayDays, @CreateDate, @Note, @Date, @ParkingActionId, @ActionId) 
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                // TODO Доделать при постановки на стоянку какие были изъяты вещи(ключи, гос номер, тех паспорт)
                //foreach (var documentType in entity.DocumentTypes)
                //{
                //    ParkingDocument parkingDocument = new ParkingDocument()
                //    {
                //        ParkingHistoryId = entity.Id,
                //        DocumentType = documentType,
                //        Date = entity.CreateDate
                //    };

                //    parkingDocument.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                //        INSERT INTO ParkingDocuments (ParkingHistoryId, DocumentType, Date) 
                //                          VALUES (@ParkingHistoryId, @DocumentType, @Date) 
                //        SELECT SCOPE_IDENTITY()", parkingDocument, UnitOfWork.Transaction);
                //}

                transaction.Commit();
            }
        }

        public void Update(ParkingHistory entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"UPDATE ParkingHistories SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ParkingHistory Find(object query)
        {            
            throw new NotImplementedException();
        }

        public ParkingHistory Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<ParkingHistory> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "WHERE p.DeleteDate IS NULL";

            var contractId = query?.Val<int?>("ContractId");

            if (contractId.HasValue) pre += " AND p.ContractId = @contractId";

            var condition = pre;
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "p.Id",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ParkingHistory, ParkingStatus, ParkingStatus, ParkingAction, ParkingHistory>($@"
                SELECT p.*, psb.*, psa.*, pa.*
                FROM ParkingHistories p
                LEFT JOIN ParkingStatuses psb ON p.StatusBeforeId=psb.Id
                LEFT JOIN ParkingStatuses psa ON p.StatusAfterId=psa.Id
                LEFT JOIN ParkingActions pa ON p.ParkingActionId=pa.Id
                {condition} {order} {page}",
                (p, psb, psa, pa) =>
                {
                    p.StatusBefore = psb;
                    p.StatusAfter = psa;
                    p.ParkingAction = pa;

                    return p;
                },
                new
                {
                    contractId,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "WHERE DeleteDate IS NULL";

            var contractId = query?.Val<int?>("ContractId");

            if (contractId.HasValue) pre += " AND ContractId = @contractId";

            var condition = pre;

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM ParkingHistories
                {condition}",new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public ParkingHistory GetActiveLastByContractId (int contractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ParkingHistory>(@"
                SELECT TOP 1 * FROM ParkingHistories 
                WHERE ContractId=@contractId 
                ORDER BY Id DESC", new { contractId }, UnitOfWork.Transaction);
        }
        
        public ParkingHistory GetDeletedByContractId (int contractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ParkingHistory>(@"
                SELECT TOP 1 * FROM ParkingHistories 
                WHERE ContractId=@contractId AND DeleteDate IS NULL 
                ORDER BY Id DESC", new { contractId }, UnitOfWork.Transaction);
        }
    }
}
