using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ContractKdnDetailRepository : RepositoryBase, IRepository<ContractKdnDetail>
    {
        public ContractKdnDetailRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM ContractKdnDetails WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractKdnDetail Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractKdnDetail Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractKdnDetail>(@"
                SELECT *
                FROM ContractKdnDetails
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ContractKdnDetail entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractKdnDetails ( ContractId, ClientId, SubjectTypeId, MonthlyPaymentAmount, OverdueAmount, FileRowId, AuthorId, CreateDate, CreditorName,
                                ContractNumber, ContractStartDate, ContractEndDate, ContractTotalAmount, ForthcomingPaymentCount, OutstandingAmount, OverdueDaysCount,
                                CollateralType, CollateralCost, IsLoanPaid, IsCreditCard, DateUpdated, UserUpdated, IsFromAdditionRequest, AdditionRequestDate, Amount4Kdn)
                    VALUES ( @ContractId, @ClientId, @SubjectTypeId, @MonthlyPaymentAmount, @OverdueAmount, @FileRowId, @AuthorId, @CreateDate, @CreditorName,
                                @ContractNumber, @ContractStartDate, @ContractEndDate, @ContractTotalAmount, @ForthcomingPaymentCount, @OutstandingAmount, @OverdueDaysCount,
                                @CollateralType, @CollateralCost, @IsLoanPaid, @IsCreditCard, @DateUpdated, @UserUpdated, @IsFromAdditionRequest, @AdditionRequestDate, @Amount4Kdn)
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractKdnDetail> GetListByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<ContractKdnDetail, FileRow, ContractKdnDetail>(@"
                SELECT ckd.*, f.*
                FROM ContractKdnDetails ckd
                LEFT JOIN FileRows f ON f.Id = ckd.FileRowId
                WHERE ckd.ContractId = @contractId AND ckd.DeleteDate IS NULL",
                (ckd, f) =>
                {
                    ckd.FileRow = f;
                    return ckd;
                },
                new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public List<ContractKdnDetail> GetListByClientIdAndContractId(int contractId, int clientId)
        {
            return UnitOfWork.Session.Query<ContractKdnDetail, FileRow, ContractKdnDetail>(@"
                SELECT ckd.*, f.*
                FROM ContractKdnDetails ckd
                LEFT JOIN FileRows f ON f.Id = ckd.FileRowId
                WHERE ckd.ClientId = @clientId AND ckd.ContractId = @contractId AND ckd.DeleteDate IS NULL
                AND CONVERT(Date, ckd.CreateDate) = CONVERT(Date, getdate())",
                (ckd, f) =>
                {
                    ckd.FileRow = f;
                    return ckd;
                },
                new { clientId, contractId }, UnitOfWork.Transaction).ToList();
        }

        public List<ContractKdnDetail> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "ContractId",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractKdnDetail>($@"
                SELECT *
                FROM ContractKdnDetails
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM ContractKdnDetails
                {condition}",
                new
                {
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }
        public void Update(ContractKdnDetail entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractKdnDetails
                    SET FileRowId = @FileRowId, IsLoanPaid = @IsLoanPaid, DateUpdated = @DateUpdated, UserUpdated = @UserUpdated
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void UpdateContractId(ContractKdnDetail entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractKdnDetails
                    SET ContractId = @ContractId
                    WHERE Id = @Id AND IsFromAdditionRequest = @IsFromAdditionRequest", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public bool FcbContractsExists(int contractId, int clientId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<bool>(@"
                IF(EXISTS(SELECT *
                FROM ContractKdnDetails
                WHERE ContractId = @contractId AND ClientId = @clientId AND CONVERT(Date, CreateDate) = CONVERT(Date, getdate())))
                BEGIN SELECT 1 END
                ELSE BEGIN SELECT 0 END", new { contractId, clientId }, UnitOfWork.Transaction);
        }

        public List<ContractKdnDetail> GetTodayListByClientIdAndContractId(int contractId, int clientId)
        {
            return UnitOfWork.Session.Query<ContractKdnDetail, FileRow, ContractKdnDetail>(@"
                SELECT ckd.*, f.*
                FROM ContractKdnDetails ckd
                LEFT JOIN FileRows f ON f.Id = ckd.FileRowId
                WHERE ckd.ClientId = @clientId AND ckd.ContractId = @contractId AND ckd.DeleteDate IS NULL AND CONVERT(Date, ckd.CreateDate) = CONVERT(Date, getdate())",
                (ckd, f) =>
                {
                    ckd.FileRow = f;
                    return ckd;
                },
                new { clientId, contractId }, UnitOfWork.Transaction).ToList();
        }

        public void DeleteTodayListByClientIdAndContractId(int contractId, int clientId)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE ContractKdnDetails
                SET DeleteDate = GETDATE()
                WHERE ClientId = @clientId AND ContractId = @contractId AND DeleteDate IS NULL AND CONVERT(Date, CreateDate) = CONVERT(Date, getdate())",
                new { clientId, contractId }, UnitOfWork.Transaction);
        }
    }
}
