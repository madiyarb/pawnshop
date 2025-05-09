using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Expenses;
using Dapper;
using Pawnshop.Data.Models.CashOrders;
using System.Threading.Tasks;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.Contracts.Views;

namespace Pawnshop.Data.Access
{
    public class ContractExpenseRepository : RepositoryBase, IRepository<ContractExpense>
    {
        public ContractExpenseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            int? contractId = query?.Val<int?>("ContractId");
            bool? isPayed = query?.Val<bool?>("IsPayed");
            var pre = "ce.DeleteDate IS NULL";
            pre += contractId.HasValue ? " AND ce.ContractId = @contractId" : string.Empty;
            pre += isPayed.HasValue ? " AND ce.IsPayed = @isPayed" : string.Empty;
            var condition = listQuery.Like(pre, "ce.Name");
            var page = listQuery.Page();

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM ContractExpenses1 ce WITH(NOLOCK)
                {condition} {page}", new
            {
                listQuery.Filter,
                contractId,
                isPayed,
            }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractExpenses1 SET
                        DeleteDate = dbo.GETASTANADATE()
                    WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ContractExpense Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractExpense Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractExpense>(@"
                SELECT *
                FROM ContractExpenses1
                WHERE Id = @id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public async Task<ContractExpense> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ContractExpense>(@"
                SELECT *
                FROM ContractExpenses1
                WHERE Id = @id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ContractExpense entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractExpenses1 ( Date, ExpenseId, ContractId, TotalCost, Reason, Name, Note, AuthorId, CreateDate, TotalLeft, 
                        RemittanceId, IsPayed, UserId)
                    VALUES ( @Date, @ExpenseId, @ContractId, @TotalCost, @Reason, @Name, @Note, @AuthorId, @CreateDate, @TotalLeft, 
                        @RemittanceId, @IsPayed, @UserId)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractExpense> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            int? contractId = query?.Val<int?>("ContractId");
            bool? isPayed = query?.Val<bool?>("IsPayed");

            var pre = "ce.DeleteDate IS NULL";

            pre += contractId.HasValue ? " AND ce.ContractId = @contractId" : string.Empty;
            pre += isPayed.HasValue ? " AND ce.IsPayed = @isPayed" : string.Empty;
            var condition = listQuery.Like(pre, "ce.Name");
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractExpense>($@"
                SELECT ce.*
                FROM ContractExpenses1 ce WITH(NOLOCK)
                {condition} {page}", new
            {
                listQuery.Filter,
                contractId,
                isPayed,
            }, UnitOfWork.Transaction).ToList();
        }

        public async Task<List<ContractExpense>> GetContractExpenseAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };

            var sqlQuery = @"
                SELECT ce.*,
                       cer.*,
                       cero.*,
                       co.*,
                       da.*,
                       ca.*
                FROM ContractExpenses1 ce
                LEFT JOIN ContractExpenseRows cer ON cer.ContractExpenseId = ce.Id AND cer.DeleteDate IS NULL
                LEFT JOIN ContractExpenseRowOrders cero ON cero.ContractExpenseRowId = cer.Id AND cero.DeleteDate IS NULL
                LEFT JOIN CashOrders co ON co.Id = cero.OrderId
                LEFT JOIN Accounts da ON da.Id = co.DebitAccountId
                LEFT JOIN Accounts ca ON ca.Id = co.CreditAccountId
                WHERE ce.ContractId = @ContractId 
                  AND ce.DeleteDate IS NULL
                ORDER BY ce.Date";

            var contractExpenseDictionary = new Dictionary<int, ContractExpense>();
            var contractExpenseRowDictionary = new Dictionary<int, ContractExpenseRow>();

            var expenses = await UnitOfWork.Session
                .QueryAsync<ContractExpense, ContractExpenseRow, ContractExpenseRowOrder, CashOrder, Account, Account, ContractExpense>(
                    sqlQuery,
                    (ce, cer, cero, co, da, ca) =>
                    {
                        if (!contractExpenseDictionary.TryGetValue(ce.Id, out var contractExpense))
                        {
                            contractExpense = ce;
                            contractExpense.ContractExpenseRows = new List<ContractExpenseRow>();
                            contractExpenseDictionary.Add(ce.Id, contractExpense);
                        }

                        if (cer != null)
                        {
                            if (!contractExpenseRowDictionary.TryGetValue(cer.Id, out var contractExpenseRow))
                            {
                                contractExpenseRow = cer;
                                contractExpenseRow.ContractExpenseRowOrders = new List<ContractExpenseRowOrder>();
                                contractExpense.ContractExpenseRows.Add(contractExpenseRow);
                                contractExpenseRowDictionary.Add(cer.Id, contractExpenseRow);
                            }

                            if (cero != null)
                            {
                                if (co != null)
                                {
                                    co.DebitAccount = da;
                                    co.CreditAccount = ca;
                                }

                                cero.Order = co;
                                contractExpenseRow.ContractExpenseRowOrders.Add(cero);
                            }
                        }

                        return contractExpense;
                    },
                    parameters,
                    UnitOfWork.Transaction,
                    splitOn: "Id,Id,Id,Id,Id");

            return expenses.Distinct().ToList();
        }

        public void Update(ContractExpense entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractExpenses1
                    SET 
                        Date = @Date,
                        ExpenseId = @ExpenseId,
                        ContractId = @ContractId,
                        TotalCost = @TotalCost,
                        Reason = @Reason,
                        Name = @Name, 
                        Note = @Note,
                        AuthorId = @AuthorId,
                        CreateDate = @CreateDate,
                        TotalLeft = @TotalLeft,
                        RemittanceId = @RemittanceId,
                        IsPayed = @IsPayed,
                        UserId = @UserId
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task<IEnumerable<ContractExpense>> GetListByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryAsync<ContractExpense>(@"SELECT ce.*
  FROM ContractExpenses1 ce WITH(NOLOCK)
 WHERE ce.ContractId = @contractId
   AND ce.DeleteDate IS NULL
 ORDER BY ce.Date",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<ContractExpenseOnlineInfo>> GetContractExpenseInfoForOnline(List<int> contractIds)
        {
            SqlBuilder builder = new SqlBuilder();

            #region Select

            builder.Select("ContractExpenses1.ContractId");
            builder.Select("CASE WHEN ContractExpenses1.IsPayed = 0 THEN 1 ELSE 0 END AS HasAdditionalExpenses");

            #endregion


            #region Join

            builder.LeftJoin(@"Expenses ON Expenses.Id = ContractExpenses1.ExpenseId");

            #endregion

            #region Where

            builder.Where("ContractExpenses1.ContractId IN @contractIds", new { contractIds = contractIds });
            builder.Where("Expenses.ExtraExpense = 1");
            builder.Where("ContractExpenses1.IsPayed = 0");
            #endregion


            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM ContractExpenses1 
             /**leftjoin**/ /**where**/ /**orderby**/ ");

            return await UnitOfWork.Session.QueryAsync<ContractExpenseOnlineInfo>(selector.RawSql, selector.Parameters);
        }

        public async Task<IEnumerable<ContractExpense>> GetUnpaidExpensesAsync(int contractId)
        {
            var expenses = await GetContractExpenseAsync(contractId);
            return expenses.Where(e => !e.IsPayed);
        }
    }
}
