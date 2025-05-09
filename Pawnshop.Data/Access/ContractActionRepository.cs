using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using System;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Membership;
using Pawnshop.AccountingCore.Models;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ContractActionRepository : RepositoryBase, IRepository<ContractAction>
    {
        private readonly ClientRepository _clientRepository;
        private readonly LoanSubjectRepository _loanSubjectRepository;

        public ContractActionRepository(IUnitOfWork unitOfWork, ClientRepository clientRepository, LoanSubjectRepository loanSubjectRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
            _loanSubjectRepository = loanSubjectRepository;
        }

        public void Insert(ContractAction entity)
        {
            entity.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
                    INSERT INTO ContractActions
                           (ContractId, ActionType, Date, TotalCost, Note, Reason, Data, 
                            FollowedId, AuthorId, CreateDate, ProcessingId, ProcessingType, 
                            OnlinePaymentId, ExpenseId,EmployeeId, Cost, ParentActionId, 
                            ChildActionId, RefinanceConfig, PayTypeId, PayOperationId, 
                            RequisiteId, ExtraExpensesCost, IsInitialFee, LoanSubjectId, 
                            Status, SellingId, BuyoutCreditLine, ClientDefermentId)
                    VALUES (@ContractId, @ActionType, @Date, @TotalCost, @Note, @Reason, @Data, 
                            @FollowedId, @AuthorId, @CreateDate, @ProcessingId, @ProcessingType, 
                            @OnlinePaymentId, @ExpenseId, @EmployeeId, @Cost, @ParentActionId, 
                            @ChildActionId, @RefinanceConfig, @PayTypeId, @PayOperationId, 
                            @RequisiteId, @ExtraExpensesCost, @IsInitialFee, @LoanSubjectId, 
                            @Status, @SellingId, @BuyoutCreditLine, @ClientDefermentId)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(ContractAction entity)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE ContractActions 
                SET 
                    ChildActionId=@ChildActionId, 
                    ParentActionId=@ParentActionId, 
                    Data=@Data, TotalCost=@TotalCost, 
                    PayOperationId=@PayOperationId, 
                    ExtraExpensesCost=@ExtraExpensesCost, 
                    ExpenseId=@ExpenseId, 
                    IsInitialFee = @IsInitialFee, 
                    LoanSubjectId = @LoanSubjectId, 
                    Status = @Status, 
                    SellingId = @SellingId,
                    Reason = @Reason,
                    FollowedId = @FollowedId,
                    BuyoutCreditLine = @BuyoutCreditLine,
                    ClientDefermentId = @ClientDefermentId
                WHERE Id=@Id", entity, UnitOfWork.Transaction);
        }
        
        public async Task UpdateAsync(ContractAction entity)
        {
            await UnitOfWork.Session.ExecuteAsync(@"
                UPDATE ContractActions 
                SET 
                    ChildActionId=@ChildActionId, 
                    ParentActionId=@ParentActionId, 
                    Data=@Data, TotalCost=@TotalCost, 
                    PayOperationId=@PayOperationId, 
                    ExtraExpensesCost=@ExtraExpensesCost, 
                    ExpenseId=@ExpenseId, 
                    IsInitialFee = @IsInitialFee, 
                    LoanSubjectId = @LoanSubjectId, 
                    Status = @Status, 
                    SellingId = @SellingId,
                    Reason = @Reason,
                    FollowedId = @FollowedId,
                    BuyoutCreditLine = @BuyoutCreditLine
                WHERE Id=@Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE ContractActions 
                    SET DeleteDate = dbo.GETASTANADATE() 
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ContractAction Get(int id)
        {
            var action = UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();

            action.Rows = UnitOfWork.Session.Query<ContractActionRow, Account, Account, ContractActionRow>(@"
                SELECT car.*, da.*, ca.*
                FROM ContractActionRows car WITH(NOLOCK)
                LEFT JOIN Accounts da ON car.DebitAccountId = da.Id
                LEFT JOIN Accounts ca ON car.CreditAccountId = ca.Id
                WHERE car.ActionId = @id",
                (car, da, ca) =>
                {
                    car.DebitAccount = da;
                    car.CreditAccount = ca;
                    return car;
                },
                new { id = action.Id }, UnitOfWork.Transaction).ToArray();

            action.Files = UnitOfWork.Session.Query<FileRow>(@"
                SELECT fr.* FROM FileRows fr
                JOIN ContractFileRows cfr ON cfr.FileRowId=fr.Id
                WHERE cfr.DeleteDate IS NULL AND cfr.ActionId= @id", new { id = action.Id }, UnitOfWork.Transaction).ToList();

            action.Checks = UnitOfWork.Session.Query<ContractActionCheckValue, ContractActionCheck, ContractActionCheckValue>(@"
                SELECT val.*, ch.* FROM ContractActionCheckValues val
                LEFT JOIN ContractActionChecks ch ON ch.Id = val.CheckId
                WHERE val.ActionId = @id",
                (val, ch) =>
                {
                    val.Check = ch;
                    return val;
                },
                new { id = action.Id }, UnitOfWork.Transaction).ToList();

            if (action.ExpenseId > 0)
            {
                action.Expense = UnitOfWork.Session.Query<ContractExpense>(@"
                    SELECT *
                    FROM ContractExpenses1
                    WHERE Id = @id", new { id = action.ExpenseId }, UnitOfWork.Transaction).FirstOrDefault();
            }

            action.Discount = new ContractDutyDiscount();
            action.Discount.Discounts = UnitOfWork.Session.Query<Discount, Blackout, ContractDiscount, PersonalDiscount, Discount>($@"
                SELECT d.*, b.*, cd.*, pd.*
                  FROM Discounts d
                  LEFT JOIN Blackouts b ON b.Id = d.BlackoutId
                  LEFT JOIN ContractDiscounts cd ON cd.Id = d.ContractDiscountId
                  LEFT JOIN PersonalDiscounts pd ON pd.Id = cd.PersonalDiscountId
                WHERE d.ActionId = @id",
                (d, b, cd, pd) =>
                {
                    d.Blackout = b;
                    d.ContractDiscount = cd;
                    if (d.ContractDiscount != null)
                    {
                        d.ContractDiscount.PersonalDiscount = pd;
                    }
                    return d;
                }, new
                {
                    id = action.Id
                }, UnitOfWork.Transaction).ToList();
            action.Discount.Discounts.ForEach(discount =>
            {
                discount.Rows = UnitOfWork.Session.Query<DiscountRow>($@"
                SELECT *
                  FROM DiscountRows
                WHERE DiscountId = @id", new
                {
                    id = discount.Id
                }, UnitOfWork.Transaction).ToList();
            });
            return action;
        }

        public List<ContractAction> GetActions(int sellingId)
        {
            var actions = UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.SellingId = @sellingId", new { sellingId }, UnitOfWork.Transaction).ToList();

            var resultActions = new List<ContractAction>();
            actions.ForEach(action =>
            {
                resultActions.Add(Get(action.Id));
            });

            return resultActions;
        }

        public ContractAction GetLastContractActionByType(int contractId, ContractActionType contractActionType)
        {
            return UnitOfWork.Session.Query<ContractAction>(@"
                SELECT TOP 1 *
                  FROM ContractActions ca
                WHERE ca.ContractId = @contractId
                AND ca.ActionType = @contractActionType
                AND ca.DeleteDate IS NULL ORDER BY Id DESC",
                new { contractId, contractActionType }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ContractLoanSubject GetLoanSubject(int subjectId)
        {
            return UnitOfWork.Session.Query<ContractLoanSubject, User, ContractLoanSubject>(@"SELECT sub.*, u.*
FROM ContractLoanSubjects sub
LEFT JOIN Users u ON sub.AuthorId = u.Id
WHERE sub.Id = @subjectId", (sub, u) =>
            {
                sub.Author = u;
                sub.Client = _clientRepository.Get(sub.ClientId);
                sub.Subject = _loanSubjectRepository.Get(sub.SubjectId);
                return sub;
            }, new { subjectId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ContractAction Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public DateTime FindLastDateInterestAccrualOnOverdue(DateTime nextPaymentDate, DateTime accrualDate, int contractId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<DateTime>(@"
                SELECT TOP 1 Date FROM ContractActions
                    WHERE DeleteDate IS NULL
                        AND ContractId = @contractId
                            AND ActionType = 141
                                AND Date BETWEEN @nextPaymentDate AND @accrualDate
                                    ORDER BY Date DESC",
                new { nextPaymentDate, accrualDate, contractId }, UnitOfWork.Transaction);
        }

        public List<ContractAction> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public List<ContractAction> ListForProcessing(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var date = query?.Val<DateTime?>("Date");
            var processingType = query?.Val<ProcessingType>("ProcessingType");

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "contractId",
                Direction = SortDirection.Asc
            });

            return UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions
                WHERE DeleteDate IS NULL 
                  AND ProcessingId IS NOT NULL 
                  AND ProcessingType=@processingType 
                  AND Date=@date 
                  {order}", new
            {
                date,
                processingType
            }, UnitOfWork.Transaction).ToList();
        }

        public ContractAction FindByProcessing(long processingId, ProcessingType processingType)
        {
            var exists = UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ContractActions WHERE ProcessingId = @processingId AND ProcessingType = @processingType", new
            {
                processingId,
                processingType
            });
            if (exists > 0)
            {
                var action = UnitOfWork.Session.Query<ContractAction>(@"
                SELECT * FROM ContractActions WHERE processingId = @processingId AND ProcessingType = @ProcessingType", new { processingId, processingType }).FirstOrDefault();

                action.Rows = UnitOfWork.Session.Query<ContractActionRow>(@"
                SELECT * FROM ContractActionRows WHERE ActionId=@id", new { id = action.Id }).ToArray();

                return action;
            }
            else
            {
                return new ContractAction();
            }
        }

        public async Task<List<ContractAction>> GetByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.ContractId = @contractId AND t.DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public async Task<IEnumerable<ContractAction>> GetByContractIdAndDates(int contractId, DateTime startDate, DateTime endDate)
        {
            return await UnitOfWork.Session.QueryAsync<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.ContractId = @contractId 
                AND t.Date BETWEEN @startDate AND @endDate
                AND t.DeleteDate IS NULL", new { contractId, startDate, endDate }, UnitOfWork.Transaction);
        }

        public async Task<List<ContractAction>> GetByParentId(int parentId)
        {
            return UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.ParentActionId = @parentId AND t.DeleteDate IS NULL", new { parentId }, UnitOfWork.Transaction).ToList();
        }

        public async Task<List<ContractAction>> GetByChildId(int childId)
        {
            return UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.ChildActionId = @childId AND t.DeleteDate IS NULL", new { childId }, UnitOfWork.Transaction).ToList();
        }

        public async Task<List<ContractAction>> GetIncompleteActions(int contractId)
        {
            return UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.ContractId = @contractId AND t.Status in (0, 15) AND t.DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public async Task<List<ContractAction>> GetAllAwaitingForApproveActions()
        {
            return UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.Status = 0 AND t.DeleteDate IS NULL AND t.CreateDate >= {DateTime.Now.Date}", UnitOfWork.Transaction).ToList();
        }

        public async Task<List<ContractAction>> GetAllAwaitingForCancelActions()
        {
            return UnitOfWork.Session.Query<ContractAction>($@"
                SELECT *
                  FROM ContractActions t WITH(NOLOCK)
                WHERE t.Status = 15 AND t.DeleteDate IS NULL AND t.CreateDate >= {DateTime.Now.Date}", UnitOfWork.Transaction).ToList();
        }

        public async Task<bool> FollowedActionsExist(int ActionId, int Contractid)
        {
            var count = UnitOfWork.Session.ExecuteScalar<int>($@"                
                SELECT count(*)
                FROM ContractActions t WITH(NOLOCK)
                WHERE Id > @ActionId and t.DeleteDate IS NULL
                and ContractId = @Contractid
                and Status in (10,15)", new { ActionId, Contractid }, UnitOfWork.Transaction);

            return count > 0;
        }

        public async Task<ContractAction> GetSignAction(List<int> relatedActions)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ContractAction>(@"SELECT *
  FROM ContractActions
 WHERE DeleteDate IS NULL
   AND Id IN @relatedActions
   AND ActionType = @actionType",
               new { relatedActions, actionType = ContractActionType.Sign }, UnitOfWork.Transaction);
        }
    }
}