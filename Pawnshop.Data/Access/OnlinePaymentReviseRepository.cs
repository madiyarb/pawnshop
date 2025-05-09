using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises;
using Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises.OnlinePaymentRevisePows;


namespace Pawnshop.Data.Access
{
    public class OnlinePaymentReviseRepository : RepositoryBase, IRepository<OnlinePaymentRevise>
    {
        private readonly ClientRepository _clientRepository;
        private readonly LoanProductTypeRepository _loanProductTypeRepository;

        public OnlinePaymentReviseRepository(IUnitOfWork unitOfWork, 
            ClientRepository clientRepository, 
            LoanProductTypeRepository loanProductTypeRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
            _loanProductTypeRepository = loanProductTypeRepository;
        }

        public void Insert(OnlinePaymentRevise entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO OnlinePaymentRevises ( CreateDate, ProcessingType, Status, DeleteDate, AuthorId, TransactionDate )
                    VALUES ( @CreateDate, @ProcessingType, @Status, @DeleteDate, @AuthorId, @TransactionDate )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                if (entity.Rows != null) InsertRows(entity);

                transaction.Commit();
            }
        }

        public void InsertRows(OnlinePaymentRevise entity)
        {
            using (var transaction = BeginTransaction())
            {
                //entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                //    INSERT INTO OnlinePaymentRevises ( CreateDate, ProcessingType, Status, DeleteDate, AuthorId )
                //    VALUES ( @CreateDate, @ProcessingType, @Status, @DeleteDate, @AuthorId )
                //    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                foreach (var row in entity.Rows)
                {
                    row.ReviseId = entity.Id;
                    UnitOfWork.Session.Execute(@"
                        INSERT INTO OnlinePaymentReviseRows
                        ( ReviseId, ProcessingId, Amount, CompanyBin, OrganizationId, ContractId, ActionId, Status, Message, DeleteDate, Date )
                        VALUES ( @ReviseId, @ProcessingId, @Amount, @CompanyBin, @OrganizationId, @ContractId, @ActionId, @Status, @Message, @DeleteDate, @Date )",
                        row, UnitOfWork.Transaction);
                }

                transaction.Commit();
            }
        }

        public void Update(OnlinePaymentRevise entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"UPDATE OnlinePaymentRevises SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
            UnitOfWork.Session.Execute(@"UPDATE OnlinePaymentReviseRows SET DeleteDate = dbo.GETASTANADATE() WHERE ReviseId = @id", new { id }, UnitOfWork.Transaction);
        }

        public OnlinePaymentRevise Get(int id)
        {
            var entity = UnitOfWork.Session.Query<OnlinePaymentRevise, User, OnlinePaymentRevise>(@"
                    SELECT o.*, u.* 
                    FROM OnlinePaymentRevises o
                        LEFT JOIN Users u ON o.AuthorId = u.Id
                    WHERE o.Id = @id
                ", (c, u) =>
                    {
                        c.Author = u;
                        return c;
                    }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null)
                throw new PawnshopApplicationException($"Результат сверки с платежной системы не найден по {nameof(OnlinePaymentRevise.Id)} {id}");

            entity.Rows = UnitOfWork.Session.Query<OnlinePaymentReviseRow, Contract, ContractAction, OnlinePaymentReviseRow>(@"
                SELECT o.*, c.*, ca.*
                FROM OnlinePaymentReviseRows o
                    LEFT JOIN Contracts c ON c.Id = o.ContractId
                    LEFT JOIN ContractActions ca ON ca.Id = o.ActionId
                 WHERE ReviseId = @id", (o, c, ca) =>
                    {
                        o.Contract = c;
                        o.Contract.Client = _clientRepository.GetOnlyClient(c.ClientId);
                        o.Contract.ProductType = o.Contract.ProductTypeId.HasValue ? _loanProductTypeRepository.Get(o.Contract.ProductTypeId.Value) : o.Contract.ProductType;
                        o.Action = ca;
                        return o;
                    }, new { id }, UnitOfWork.Transaction).ToList();

            return entity;
        }

        public OnlinePaymentRevise Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<OnlinePaymentRevise> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var processingType = query?.Val<ProcessingType?>("ProcessingType");

            var pre = "c.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND ca.Date >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND ca.Date <= @endDate" : string.Empty;
            pre += processingType.HasValue ? " AND c.ProcessingType = @processingType" : string.Empty;

            var condition = listQuery.Like(pre, "c.CreateDate", "c.ProcessingType");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "CreateDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<OnlinePaymentRevise, User, OnlinePaymentRevise>($@"
                SELECT DISTINCT c.*, u.*
                FROM OnlinePaymentRevises c
                JOIN Users u ON c.AuthorId = u.Id
                JOIN OnlinePaymentReviseRows oprs ON oprs.ReviseId = c.Id
                LEFT JOIN ContractActions ca ON oprs.ActionId = ca.Id
                {condition} {order} {page}", 
                (c, u) =>
                {
                    c.Author = u;
                    return c;
                }, new
                {
                    beginDate,
                    endDate,
                    processingType,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var processingType = query?.Val<ProcessingType?>("ProcessingType");

            var pre = "c.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND ca.Date >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND ca.Date <= @endDate" : string.Empty;
            pre += processingType.HasValue ? " AND c.ProcessingType = @processingType" : string.Empty;

            var condition = listQuery.Like(pre, "c.CreateDate", "c.ProcessingType");
            
            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(DISTINCT c.Id)
                FROM OnlinePaymentRevises c
                JOIN Users u ON c.AuthorId = u.Id
                JOIN OnlinePaymentReviseRows oprs ON oprs.ReviseId = c.Id
                LEFT JOIN ContractActions ca ON oprs.ActionId = ca.Id
                {condition}", new
            {
                    beginDate,
                    endDate,
                    processingType,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }
    }
}
