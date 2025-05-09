using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Sellings;

namespace Pawnshop.Data.Access
{
    public class SellingRepository : RepositoryBase, IRepository<Selling>
    {
        private readonly LoanProductTypeRepository _loanProductTypeRepository;

        public SellingRepository(IUnitOfWork unitOfWork, LoanProductTypeRepository loanProductTypeRepository) : base(unitOfWork)
        {
            _loanProductTypeRepository = loanProductTypeRepository;
        }

        public void Insert(Selling entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Sellings
( CollateralType, CreateDate, ContractId, ContractPositionId, PositionId, PriceCost, Note, PositionSpecific,
SellingCost, SellingDate, CashOrderId, Status, OwnerId, BranchId, AuthorId, DeleteDate )
VALUES ( @CollateralType, @CreateDate, @ContractId, @ContractPositionId, @PositionId, @PriceCost, @Note, @PositionSpecific,
@SellingCost, @SellingDate, @CashOrderId, @Status, @OwnerId, @BranchId, @AuthorId, @DeleteDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Selling entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Sellings
                        SET CollateralType = @CollateralType, CreateDate = @CreateDate, ContractId = @ContractId, ContractPositionId = @ContractPositionId,
                        PositionId = @PositionId, PriceCost = @PriceCost, Note = @Note, PositionSpecific = @PositionSpecific,
                        SellingCost = @SellingCost, SellingDate = @SellingDate, CashOrderId = @CashOrderId, Status = @Status,
                        OwnerId = @OwnerId, BranchId = @BranchId, AuthorId = @AuthorId, DeleteDate = @DeleteDate
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Sellings SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new {id}, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Selling GetByContractId(int contractId)
        {
            var selling = UnitOfWork.Session.Query<Selling, Contract, Selling>(@"
                SELECT s.*, c.*
                FROM Sellings s
                LEFT JOIN Contracts c ON s.ContractId = c.Id
                WHERE s.ContractId = @contractId",
                (s, c) =>
                {
                    s.Contract = c;
                    s.Contract.ProductType = s.Contract.ProductTypeId.HasValue ? _loanProductTypeRepository.Get(s.Contract.ProductTypeId.Value) : s.Contract.ProductType;
                    return s;
                },
                new { contractId }).ToList().FirstOrDefault();

            selling.SellingRows = UnitOfWork.Session.Query<SellingRow>(@"
                SELECT sr.*
                FROM SellingRows sr
                WHERE sr.DeleteDate IS NULL AND sr.SellingId = @id", new { id = selling.Id }, UnitOfWork.Transaction).ToList();

            if (selling.SellingRows.Any())
            {
                selling.ActionRows = UnitOfWork.Session.Query<ContractActionRow>(@"
                    SELECT car.*
                    FROM ContractActionRows car
                    WHERE car.ActionId = @id", new { id = selling.SellingRows.FirstOrDefault().ActionId }, UnitOfWork.Transaction).ToList();
            }

            return selling;
        }

        public Selling Get(int id)
        {
            var selling = UnitOfWork.Session.Query<Selling, Contract, Selling>(@"
                SELECT s.*, c.*
                FROM Sellings s
                LEFT JOIN Contracts c ON s.ContractId = c.Id
                WHERE s.Id = @id",
                (s, c) =>
                {
                    s.Contract = c;
                    s.Contract.ProductType = s.Contract.ProductTypeId.HasValue ? _loanProductTypeRepository.Get(s.Contract.ProductTypeId.Value) : s.Contract.ProductType;
                    return s;
                },
                new { id }, UnitOfWork.Transaction).ToList().FirstOrDefault();

            selling.SellingRows = UnitOfWork.Session.Query<SellingRow>(@"
                SELECT sr.*
                FROM SellingRows sr
                WHERE sr.DeleteDate IS NULL AND sr.SellingId = @id", new { id = selling.Id }, UnitOfWork.Transaction).ToList();

            if (selling.SellingRows.Any())
            {
                selling.ActionRows = UnitOfWork.Session.Query<ContractActionRow>(@"
                    SELECT car.*
                    FROM ContractActionRows car
                    WHERE car.ActionId = @id", new { id = selling.SellingRows.FirstOrDefault().ActionId }, UnitOfWork.Transaction).ToList();
            }

            return selling;
        }

        public Selling Find(object query)
        {
            var contractPositionId = query?.Val<int>("ContractPositionId");
            var pre = "Sellings.DeleteDate IS NULL";
            pre += contractPositionId.HasValue ? " AND Sellings.ContractPositionId = @contractPositionId" : string.Empty;

            return UnitOfWork.Session.QueryFirstOrDefault<Selling>($@"
SELECT *
FROM Sellings
WHERE {pre}", new { contractPositionId }, UnitOfWork.Transaction);
        }

        public List<Selling> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var status = query?.Val<SellingStatus?>("Status");            
            var ownerId = query?.Val<int?>("OwnerId");
            var isAllBranchesList = query.Val<bool>("IsAllBranchesList");

            var pre = "Sellings.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND Sellings.CreateDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND Sellings.CreateDate <= @endDate" : string.Empty;
            pre += collateralType.HasValue ? " AND Sellings.CollateralType = @collateralType" : string.Empty;
            pre += status.HasValue ? " AND Sellings.Status = @status" : string.Empty;
            pre += !isAllBranchesList ? " AND Sellings.BranchId = @ownerId": string.Empty;

            var condition = listQuery.Like(pre, "Positions.Name", "Contracts.ContractNumber");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Sellings.CreateDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Selling, Position, Group, User, Contract, Selling>($@"
                SELECT Sellings.*, Positions.*, Groups.*, Users.*,Contracts.* 
                FROM Sellings
                    JOIN Positions ON Sellings.PositionId = Positions.Id
                    JOIN Groups ON Sellings.BranchId = Groups.Id
                    JOIN Users ON Sellings.AuthorId = Users.Id
                    LEFT JOIN Contracts ON Sellings.ContractId = Contracts.Id
                {condition} {order} {page}",
                    (s, p, g, u, c) =>
                    {
                        s.Position = p;
                        s.Branch = g;
                        s.Author = u;
                        s.Contract = c;
                        s.Contract.ProductType = s.Contract.ProductTypeId.HasValue ? _loanProductTypeRepository.Get(s.Contract.ProductTypeId.Value) : s.Contract.ProductType;

                        return s;
                    },
                    new
                    {
                        beginDate,
                        endDate,
                        collateralType,
                        status,
                        ownerId,
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
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var status = query?.Val<SellingStatus?>("Status");            
            var ownerId = query?.Val<int?>("OwnerId");
            var isAllBranchesList = query.Val<bool>("IsAllBranchesList");

            var pre = "Sellings.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND Sellings.CreateDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND Sellings.CreateDate <= @endDate" : string.Empty;
            pre += collateralType.HasValue ? " AND Sellings.CollateralType = @collateralType" : string.Empty;
            pre += status.HasValue ? " AND Sellings.Status = @status" : string.Empty;
            pre += !isAllBranchesList ? " AND Sellings.BranchId = @ownerId" : string.Empty;
            
            var condition = listQuery.Like(pre, "Positions.Name", "Contracts.ContractNumber");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(Sellings.Id) 
                FROM Sellings
                    JOIN Positions ON Sellings.PositionId = Positions.Id
                    JOIN Groups ON Sellings.BranchId = Groups.Id
                    JOIN Users ON Sellings.AuthorId = Users.Id
                    LEFT JOIN Contracts ON Sellings.ContractId = Contracts.Id
                {condition}", 
                    new
                    {
                        beginDate,
                        endDate,
                        collateralType,
                        status,
                        ownerId,
                        listQuery.Filter
                    }, UnitOfWork.Transaction);
        }

        public void DeleteRow(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE SellingRows SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void InsertRow(SellingRow entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO SellingRows
                            ( SellingId, SellingPaymentType, Cost, DebitAccountId, CreditAccountId, OrderId, ActionId, CreateDate, ContractDiscountId, ExtraExpensesCost )
                    VALUES ( @SellingId, @SellingPaymentType, @Cost, @DebitAccountId, @CreditAccountId, @OrderId, @ActionId, @CreateDate, @ContractDiscountId, @ExtraExpensesCost )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void UpdateRow(SellingRow entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    UPDATE SellingRows
                        SET SellingId = @SellingId, SellingPaymentType = @SellingPaymentType, Cost = @Cost, DebitAccountId = @DebitAccountId, 
                        CreditAccountId = @CreditAccountId, OrderId = @OrderId, ActionId = @ActionId, CreateDate = @CreateDate, ContractDiscountId = @ContractDiscountId, ExtraExpensesCost = @ExtraExpensesCost)
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}