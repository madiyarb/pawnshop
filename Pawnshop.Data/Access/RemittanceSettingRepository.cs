using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class RemittanceSettingRepository : RepositoryBase, IRepository<RemittanceSetting>
    {
        public RemittanceSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(RemittanceSetting entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO RemittanceSettings ( SendBranchId, ReceiveBranchId, CashOutDebitId, CashOutCreditId, CashInDebitId, CashInCreditId, ExpenseTypeId, CashOutUserId, CashInUserId, CashOutBusinessOperationId, CashInBusinessOperationId )
VALUES ( @SendBranchId, @ReceiveBranchId, @CashOutDebitId, @CashOutCreditId, @CashInDebitId, @CashInCreditId, @ExpenseTypeId, @CashOutUserId, @CashInUserId, @CashOutBusinessOperationId, @CashInBusinessOperationId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(RemittanceSetting entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE RemittanceSettings
SET SendBranchId = @SendBranchId, ReceiveBranchId = @ReceiveBranchId, CashOutDebitId = @CashOutDebitId, CashOutCreditId = @CashOutCreditId, 
    CashInDebitId = @CashInDebitId, CashInCreditId = @CashInCreditId, ExpenseTypeId = @ExpenseTypeId, CashOutUserId = @CashOutUserId, CashInUserId = @CashInUserId,
    CashOutBusinessOperationId = @CashOutBusinessOperationId, CashInBusinessOperationId = @CashInBusinessOperationId
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"DELETE FROM RemittanceSettings WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public RemittanceSetting Get(int id)
        {
            return UnitOfWork.Session.Query<RemittanceSetting, Group, Group, Account, Account, Account, Account, RemittanceSetting>(@"
SELECT TOP 1 rs.*, sb.*, rb.*, cod.*, coc.*, cid.*, cic.*
FROM RemittanceSettings rs
JOIN Groups sb ON rs.SendBranchId = sb.Id
JOIN Groups rb ON rs.ReceiveBranchId = rb.Id
JOIN Accounts cod ON rs.CashOutDebitId = cod.Id
JOIN Accounts coc ON rs.CashOutCreditId = coc.Id
JOIN Accounts cid ON rs.CashInDebitId = cid.Id
JOIN Accounts cic ON rs.CashInCreditId = cic.Id
WHERE rs.Id = @id", (rs, sb, rb, cod, coc, cid, cic) => {
                rs.SendBranch = sb;
                rs.ReceiveBranch = rb;
                rs.CashOutDebit = cod;
                rs.CashOutCredit = coc;
                rs.CashInDebit = cid;
                rs.CashInCredit = cic;
                return rs;
            }, new { id }).FirstOrDefault();
        }

        public async Task<RemittanceSetting> GetAsync(int id)
        {
            return UnitOfWork.Session.Query<RemittanceSetting, Group, Group, Account, Account, Account, Account, RemittanceSetting>(@"
SELECT TOP 1 rs.*, sb.*, rb.*, cod.*, coc.*, cid.*, cic.*
FROM RemittanceSettings rs
JOIN Groups sb ON rs.SendBranchId = sb.Id
JOIN Groups rb ON rs.ReceiveBranchId = rb.Id
JOIN Accounts cod ON rs.CashOutDebitId = cod.Id
JOIN Accounts coc ON rs.CashOutCreditId = coc.Id
JOIN Accounts cid ON rs.CashInDebitId = cid.Id
JOIN Accounts cic ON rs.CashInCreditId = cic.Id
WHERE rs.Id = @id", (rs, sb, rb, cod, coc, cid, cic) => {
                rs.SendBranch = sb;
                rs.ReceiveBranch = rb;
                rs.CashOutDebit = cod;
                rs.CashOutCredit = coc;
                rs.CashInDebit = cid;
                rs.CashInCredit = cic;
                return rs; 
            }, new { id }).FirstOrDefault();
        }

        public RemittanceSetting Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var sendBranchId = query.Val<int?>("SendBranchId");
            var receiveBranchId = query.Val<int?>("ReceiveBranchId");

            if (!sendBranchId.HasValue) throw new InvalidOperationException();
            if (!receiveBranchId.HasValue) throw new InvalidOperationException();

            return UnitOfWork.Session.Query<RemittanceSetting, Group, Group, Account, Account, Account, Account, RemittanceSetting>(@"
SELECT TOP 1 rs.*, sb.*, rb.*, cod.*, coc.*, cid.*, cic.*
FROM RemittanceSettings rs
JOIN Groups sb ON rs.SendBranchId = sb.Id
JOIN Groups rb ON rs.ReceiveBranchId = rb.Id
JOIN Accounts cod ON rs.CashOutDebitId = cod.Id
JOIN Accounts coc ON rs.CashOutCreditId = coc.Id
JOIN Accounts cid ON rs.CashInDebitId = cid.Id
JOIN Accounts cic ON rs.CashInCreditId = cic.Id
WHERE rs.SendBranchId = @sendBranchId
    AND rs.ReceiveBranchId = @receiveBranchId", (rs, sb, rb, cod, coc, cid, cic) => {
                rs.SendBranch = sb;
                rs.ReceiveBranch = rb;
                rs.CashOutDebit = cod;
                rs.CashOutCredit = coc;
                rs.CashInDebit = cid;
                rs.CashInCredit = cic;
                return rs;
            }, new { sendBranchId, receiveBranchId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<RemittanceSetting> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "sb.DisplayName");
            var order = "ORDER BY sb.DisplayName, rb.DisplayName";
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<RemittanceSetting, Group, Group, Account, Account, Account, Account, RemittanceSetting>($@"
SELECT rs.*, sb.*, rb.*, cod.*, coc.*, cid.*, cic.*
FROM RemittanceSettings rs
JOIN Groups sb ON rs.SendBranchId = sb.Id
JOIN Groups rb ON rs.ReceiveBranchId = rb.Id
JOIN Accounts cod ON rs.CashOutDebitId = cod.Id
JOIN Accounts coc ON rs.CashOutCreditId = coc.Id
JOIN Accounts cid ON rs.CashInDebitId = cid.Id
JOIN Accounts cic ON rs.CashInCreditId = cic.Id
{condition} {order} {page}", (rs, sb, rb, cod, coc, cid, cic) => {
                rs.SendBranch = sb;
                rs.ReceiveBranch = rb;
                rs.CashOutDebit = cod;
                rs.CashOutCredit = coc;
                rs.CashInDebit = cid;
                rs.CashInCredit = cic;
                return rs;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "sb.DisplayName");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM RemittanceSettings rs
JOIN Groups sb ON rs.SendBranchId = sb.Id
JOIN Groups rb ON rs.ReceiveBranchId = rb.Id
JOIN Accounts cod ON rs.CashOutDebitId = cod.Id
JOIN Accounts coc ON rs.CashOutCreditId = coc.Id
JOIN Accounts cid ON rs.CashInDebitId = cid.Id
JOIN Accounts cic ON rs.CashInCreditId = cic.Id
{condition}", new
            {
                listQuery.Filter
            });
        }
    }
}
