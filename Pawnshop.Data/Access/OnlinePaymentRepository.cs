using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.OnlinePayments;


namespace Pawnshop.Data.Access
{
    public class OnlinePaymentRepository : RepositoryBase, IRepository<OnlinePayment>
    {
        public OnlinePaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(OnlinePayment entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
                    INSERT INTO OnlinePayments
                        (ContractId, ProcessingId, ProcessingType, ProcessingBankName, 
                        ProcessingBankNetwork, Amount, ProcessingStatus, ContractActionId, CreateDate, 
                        FinishDate, DeleteDate)
                    VALUES  
                        (@ContractId, @ProcessingId, @ProcessingType, @ProcessingBankName, 
                        @ProcessingBankNetwork, @Amount, @ProcessingStatus, @ContractActionId, @CreateDate, 
                        @FinishDate, @DeleteDate)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(OnlinePayment entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE OnlinePayments 
                    SET 
                        ContractId = @ContractId, 
                        ProcessingId = @ProcessingId, 
                        ProcessingType = @ProcessingType, 
                        ProcessingBankName = @ProcessingBankName, 
                        ProcessingBankNetwork = @ProcessingBankNetwork, 
                        Amount = @Amount, 
                        ProcessingStatus = @ProcessingStatus, 
                        ContractActionId = @ContractActionId,    
                        CreateDate = @CreateDate,
                        FinishDate = @FinishDate,
                        DeleteDate = @DeleteDate
                    WHERE Id = @id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"UPDATE OnlinePayments SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public OnlinePayment GetByProcessing(long processingId, ProcessingType processingType)
        {
            return UnitOfWork.Session.Query<OnlinePayment>(@"
                SELECT * FROM OnlinePayments
                WHERE ProcessingId = @processingId AND ProcessingType = @processingType
                AND DeleteDate IS NULL", new { processingId, processingType }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public OnlinePayment Get(int id)
        {
            return UnitOfWork.Session.Query<OnlinePayment>(@"
                SELECT * FROM OnlinePayments
                WHERE Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public OnlinePayment Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<OnlinePayment> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public List<OnlinePayment> Select()
        {
            var neededProcessingStatuses = new List<ProcessingStatus> {
                ProcessingStatus.Created,
                ProcessingStatus.Failed
            };
            return UnitOfWork.Session.Query<OnlinePayment>(@"
                SELECT * FROM OnlinePayments o
                WHERE o.DeleteDate IS NULL 
                AND o.FinishDate IS NULL
                AND o.ProcessingStatus IN @neededProcessingStatuses
                and not exists (select Id from OnlinePayments where ProcessingId = o.ProcessingId and ProcessingStatus = 20)",
                new { neededProcessingStatuses }, UnitOfWork.Transaction).ToList();
        }
    }
}
