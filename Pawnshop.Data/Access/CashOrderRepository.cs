using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class CashOrderRepository : RepositoryBase, IRepository<CashOrder>
    {
        private readonly BusinessOperationSettingRepository _businessOperationSettingRepository;
        private readonly ClientRepository _clientRepository;
        private readonly UserRepository _userRepository;
        private readonly ClientLegalFormRepository _clientLegalFormRepository;

        public CashOrderRepository(IUnitOfWork unitOfWork,
            BusinessOperationSettingRepository businessOperationSettingRepository,
            ClientRepository clientRepository, UserRepository userRepository,
            ClientLegalFormRepository clientLegalFormRepository) : base(unitOfWork)
        {
            _businessOperationSettingRepository = businessOperationSettingRepository;
            _clientRepository = clientRepository;
            _userRepository = userRepository;
            _clientLegalFormRepository = clientLegalFormRepository;
        }

        public void Insert(CashOrder entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO CashOrders
( OrderType, OrderNumber, OrderDate, OrderCost, DebitAccountId, CreditAccountId, ClientId,
  UserId, ExpenseTypeId, Reason, ReasonKaz, Note, RegDate, OwnerId, BranchId, AuthorId, ApprovedId, ApproveStatus, ApproveDate, ProveType, ProcessingId, ProcessingType,
  OperationId, ContractId, BusinessOperationId, BusinessOperationSettingId, CurrencyId, StornoId, OrderCostNC, ContractActionId )
VALUES ( @OrderType, @OrderNumber, @OrderDate, @OrderCost, @DebitAccountId, @CreditAccountId, @ClientId,
  @UserId, @ExpenseTypeId, @Reason, @ReasonKaz, @Note, @RegDate, @OwnerId, @BranchId, @AuthorId, @ApprovedId, @ApproveStatus, @ApproveDate, @ProveType, @ProcessingId, @ProcessingType,
  @OperationId, @ContractId, @BusinessOperationId, @BusinessOperationSettingId, @CurrencyId, @StornoId, @OrderCostNC, @ContractActionId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                if (entity.OrderExpense != null)
                {
                    entity.OrderExpense.Id = entity.Id;
                    InsertOrderExpenses(entity.OrderExpense);
                }

                transaction.Commit();
            }
        }

        public void Update(CashOrder entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CashOrders
SET OrderType = @OrderType, OrderNumber = @OrderNumber, OrderDate = @OrderDate, OrderCost = @OrderCost,
DebitAccountId = @DebitAccountId, CreditAccountId = @CreditAccountId, ClientId = @ClientId, UserId = @UserId,
ExpenseTypeId = @ExpenseTypeId, Reason = @Reason, ReasonKaz = @ReasonKaz, Note = @Note, RegDate = @RegDate, OwnerId = @OwnerId, 
BranchId = @BranchId, AuthorId = @AuthorId, DeleteDate = @DeleteDate, ApprovedId=@ApprovedId, ApproveStatus=@ApproveStatus, 
ApproveDate = @ApproveDate, ProveType = @ProveType, ProcessingId = @ProcessingId, ProcessingType = @ProcessingType, OperationId = @OperationId,
ContractId = @ContractId, BusinessOperationId = @BusinessOperationId, BusinessOperationSettingId = @BusinessOperationSettingId, CurrencyId = @CurrencyId, StornoId = @StornoId, OrderCostNC = @OrderCostNC, ContractActionId = @ContractActionId
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                if (entity.OrderExpense != null)
                {
                    entity.OrderExpense.Id = entity.Id;
                    UpdateOrderExpenses(entity.OrderExpense);
                }
                else
                {
                    var orderExpense = UnitOfWork.Session.Query<OrderExpense>(@"
SELECT * FROM OrdersExpenses
WHERE Id = @id", new { id = entity.Id }, UnitOfWork.Transaction).FirstOrDefault();
                    if (orderExpense != null)
                    {
                        UnitOfWork.Session.Execute(@"
					DELETE FROM OrdersExpenses
	                WHERE Id = @Id", entity, UnitOfWork.Transaction);
                    }

                }

                transaction.Commit();
            }
        }

        private void InsertOrderExpenses(OrderExpense orderExpense)
        {
            try
            {
                orderExpense.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
INSERT INTO OrdersExpenses(Id, ArticleTypeId, BranchId)
VALUES
(@Id, @ArticleTypeId, @BranchId)
SELECT SCOPE_IDENTITY()
", orderExpense, UnitOfWork.Transaction);
            }
            catch (SqlException e)
            {
                throw new PawnshopApplicationException(e.Message);
            }
        }

        private void UpdateOrderExpenses(OrderExpense requisite)
        {
            try
            {
                UnitOfWork.Session.Execute($@"
UPDATE OrdersExpenses
SET Id = @Id, ArticleTypeId = @ArticleTypeId, BranchId = @BranchId
WHERE Id=@Id
", requisite, UnitOfWork.Transaction);
            }
            catch (SqlException e)
            {
                throw new PawnshopApplicationException(e.Message);
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE CashOrders SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void UndoDelete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CashOrders SET DeleteDate = NULL WHERE Id = @id",
                    new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public CashOrder Get(int id)
        {
            var entity = UnitOfWork.Session.Query<CashOrder, Client, User, Account, Account, User, Organization, CashOrder>(@"
SELECT co.*, c.*, u.*, da.*, ca.*, a.*, o.*
FROM CashOrders co
LEFT JOIN Clients c ON co.ClientId = c.Id
LEFT JOIN Users u ON co.UserId = u.Id
LEFT JOIN Accounts da ON co.DebitAccountId = da.Id
LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id
JOIN Users a ON co.AuthorId = a.Id
JOIN Members m ON a.Id = m.Id
JOIN Organizations o ON m.OrganizationId = o.Id
WHERE co.Id = @id",
                (co, c, u, da, ca, a, o) =>
                {
                    co.Client = c;
                    co.User = u;
                    co.DebitAccount = da;
                    co.CreditAccount = ca;
                    co.Author = a;
                    a.Organization = o;
                    return co;
                },
                new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null)
                return null;

            if (entity.ExpenseTypeId.HasValue)
            {
                entity.ExpenseType = UnitOfWork.Session.Query<ExpenseType, ExpenseGroup, ExpenseType>(@"
SELECT et.*, eg.*
FROM ExpenseTypes et
JOIN ExpenseGroups eg ON et.ExpenseGroupId = eg.Id
WHERE et.Id = @id", (et, eg) => { et.ExpenseGroup = eg; return et; }, new { id = entity.ExpenseTypeId.Value }, UnitOfWork.Transaction).FirstOrDefault();
            }

            entity.OrderExpense = GetOrderExpense(entity.Id);

            entity.Language = GetCashOrderPrintLanguageByOrderId(id).Result;
            
            entity.AuctionRequestId = UnitOfWork.Session
                .ExecuteScalar<Guid?>(
                    @"SELECT RequestId FROM AuctionPayments WHERE CashOrderId = @id",
                    new { id = entity.Id },
                    UnitOfWork.Transaction);

            return entity;
        }

        public CashOrder GetOrderByStornoId(int stornoId)
        {
            var entity = UnitOfWork.Session.Query<CashOrder>(@"
SELECT * FROM CashOrders co
WHERE co.StornoId = @stornoId
AND co.DeleteDate IS NULL", new { stornoId }, UnitOfWork.Transaction).FirstOrDefault();
            if (entity != null)
            {
                entity.OrderExpense = GetOrderExpense(entity.Id);
            }
            return entity;
        }

        public async Task<CashOrder> GetOrderByStornoIdAsync(int stornoId)
        {
            var entity = await UnitOfWork.Session.QueryFirstOrDefaultAsync<CashOrder>(@"
SELECT * FROM CashOrders co
WHERE co.StornoId = @stornoId
AND co.DeleteDate IS NULL", new { stornoId }, UnitOfWork.Transaction);
            if (entity != null)
            {
                entity.OrderExpense = await GetOrderExpenseAsync(entity.Id);
            }
            return entity;
        }

        private async Task<OrderExpense> GetOrderExpenseAsync(int id)
        {
            return (await UnitOfWork.Session.QueryAsync<OrderExpense, Group, Pawnshop.Data.Models.CashOrders.ExpenseArticleType, OrderExpense>(@"
                    SELECT oe.*, g.*, ea.*
                    FROM OrdersExpenses oe
                    JOIN ExpenseArticleTypes ea ON ea.Id = oe.ArticleTypeId
                    LEFT JOIN Groups g ON oe.BranchId = g.Id
                    WHERE oe.Id = @id",
                    (oe, g, ea) =>
                    {
                        oe.Branch = g;
                        oe.ArticleType = ea;
                        return oe;
                    },
                    new { id = id }, UnitOfWork.Transaction)).FirstOrDefault();
        }

        private OrderExpense GetOrderExpense(int id)
        {
            return UnitOfWork.Session
                .Query<OrderExpense, Group, Pawnshop.Data.Models.CashOrders.ExpenseArticleType, OrderExpense>(@"
SELECT oe.*, g.*, ea.*
FROM OrdersExpenses oe
JOIN ExpenseArticleTypes ea ON ea.Id = oe.ArticleTypeId
LEFT JOIN Groups g ON oe.BranchId = g.Id
WHERE oe.Id = @id",
                    (oe, g, ea) =>
                    {
                        oe.Branch = g;
                        oe.ArticleType = ea;
                        return oe;
                    },
                    new { id = id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<CashOrder> GetOrdersByContractActionId(int contractActionId)
        {
            var entity = UnitOfWork.Session.Query<CashOrder, User, Account, Account, User, Organization, OrderExpense, CashOrder>(@"
SELECT co.*, u.*, da.*, ca.*, a.*, o.*, oe.*
FROM CashOrders co
LEFT JOIN Users u ON co.UserId = u.Id
LEFT JOIN OrdersExpenses oe ON oe.Id = co.Id
LEFT JOIN Accounts da ON co.DebitAccountId = da.Id
LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id
JOIN Users a ON co.AuthorId = a.Id
JOIN Members m ON a.Id = m.Id
JOIN Organizations o ON m.OrganizationId = o.Id
WHERE co.DeleteDate IS NULL AND co.ApproveStatus=0
AND co.ContractActionId = @сontractActionId",
                (co, u, da, ca, a, o, oe) =>
                {
                    co.User = u;
                    co.DebitAccount = da;
                    co.CreditAccount = ca;
                    co.Author = a;
                    a.Organization = o;
                    co.OrderExpense = oe;
                    co.Language = GetCashOrderPrintLanguageByOrderId(co.Id).Result;
                    return co;
                },
                new { сontractActionId = contractActionId }, UnitOfWork.Transaction).ToList();

            return entity;
        }

        public int RelationCount(int cashOrderId)
        {
            int relationCount = 0;
            var sql = @"
                declare @table_schema nvarchar(50) = 'dbo',
                        @table_name nvarchar(50) = 'CashOrders',
                        @id int = @cashOrderId

                select  'select count(*) from ' + fk_col.TABLE_SCHEMA + '.' + fk_col.TABLE_NAME + ' t1 '
                        + ' inner join ' + @table_schema + '.' + @table_name + ' t2 '
                        + ' on t1.' + fk_col.COLUMN_NAME + ' = t2.' + pk_col.COLUMN_NAME
                        + ' where t2.' + pk_col.COLUMN_NAME + ' = ' + cast(@id as nvarchar)
                from INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk

                    join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE pk_col
                        on pk.CONSTRAINT_SCHEMA = pk_col.CONSTRAINT_SCHEMA
                        and pk.CONSTRAINT_NAME = pk_col.CONSTRAINT_NAME

                    join INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS fk 
                        on pk.CONSTRAINT_SCHEMA = fk.UNIQUE_CONSTRAINT_SCHEMA 
                        and pk.CONSTRAINT_NAME = fk.UNIQUE_CONSTRAINT_NAME

                    join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE fk_col
                        on fk_col.CONSTRAINT_SCHEMA = fk.CONSTRAINT_SCHEMA
                        and fk_col.CONSTRAINT_NAME = fk.CONSTRAINT_NAME

                where pk.TABLE_SCHEMA = @table_schema 
                    and pk.TABLE_NAME = @table_name
                    and pk.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    and fk_col.TABLE_NAME NOT IN ('AccountRecords', 'CashOrders', 'UKassaRequests', 'CashOrderPrintLanguages', 'OrdersExpenses')
                UNION Select 'select count(*) from dbo.CashOrders t1  inner join dbo.CashOrders t2  on t1.StornoId = t2.Id where t2.Id = ' + cast(@id as nvarchar) + ' AND t1.DeleteDate IS NULL'";

            List<string> sqlScriptsToCheckDependencies =
                UnitOfWork.Session.Query<string>(sql, new { cashOrderId }).ToList();
            foreach (string sqlScript in sqlScriptsToCheckDependencies)
            {
                int relatedEntetiesCount = UnitOfWork.Session.ExecuteScalar<int>(sqlScript);
                relationCount += relatedEntetiesCount;
                if (relationCount > 0)
                    break;
            }

            return relationCount;
        }

        public List<CashOrder> GetListByBusinessOperationSettingsIdAndContractId(int businessOperationSettingId, int contractId)
        {
            List<CashOrder> cashOrders = UnitOfWork.Session.Query<CashOrder>(@"
                SELECT co.*
                FROM CashOrders co
                WHERE co.BusinessOperationSettingId = @businessOperationSettingId
                AND co.DeleteDate IS NULL AND co.ContractId = @contractId",
                new { businessOperationSettingId, contractId }, UnitOfWork.Transaction).ToList();

            return cashOrders;
        }

        public decimal GetSumOfCashOrderCostByBusinessOperationSettingCodesAndContractId(List<string> codes, int contractId, DateTime date)
        {
            return UnitOfWork.Session.ExecuteScalar<decimal>(@"
                SELECT ISNULL(SUM(IIF(co.StornoId IS NULL, co.OrderCost, -co.OrderCost)), 0)
                    FROM CashOrders co
                        JOIN BusinessOperationSettings bs ON bs.Id = co.BusinessOperationSettingId
                            WHERE bs.Code IN @codes
                                AND co.DeleteDate IS NULL 
                                    AND co.ContractId = @contractId
                                        AND CAST(co.OrderDate AS DATE) <= @date",
                new { codes, contractId, date }, UnitOfWork.Transaction);

        }

        public async Task<CashOrder> GetAsync(int id)
        {
            var entities = await UnitOfWork.Session
                .QueryAsync<CashOrder, Client, User, Account, Account, User, Organization, CashOrder>(@"
SELECT co.*, op.Id AS TasOnlinePaymentId, tmfp.Id AS TMFPaymentId, c.*, u.*, da.*, ca.*, a.*, o.*
FROM CashOrders co
LEFT JOIN Clients c ON co.ClientId = c.Id
LEFT JOIN Users u ON co.UserId = u.Id
LEFT JOIN Accounts da ON co.DebitAccountId = da.Id
LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id
JOIN Users a ON co.AuthorId = a.Id
JOIN Members m ON a.Id = m.Id
JOIN Organizations o ON m.OrganizationId = o.Id
LEFT JOIN TasOnlinePayments op ON op.OrderId = co.Id
LEFT JOIN TMFPayments tmfp ON tmfp.OrderId = co.Id
WHERE co.Id = @id",
                (co, c, u, da, ca, a, o) =>
                {
                    co.Client = c;
                    co.User = u;
                    co.DebitAccount = da;
                    co.CreditAccount = ca;
                    co.Author = a;
                    a.Organization = o;
                    co.Language = GetCashOrderPrintLanguageByOrderId(co.Id).Result;
                    return co;
                },
                new { id }, UnitOfWork.Transaction);

            if (entities == null)
                return null;
            if (!entities.Any())
                return null;

            var entity = entities.FirstOrDefault();
            if (entity.ExpenseTypeId.HasValue)
            {
                var result = await UnitOfWork.Session
                    .QueryAsync<ExpenseType, ExpenseGroup, ExpenseType>(@"
                        SELECT et.*, eg.*
                        FROM ExpenseTypes et
                        JOIN ExpenseGroups eg ON et.ExpenseGroupId = eg.Id
                        WHERE et.Id = @id",
                        (et, eg) =>
                        {
                            et.ExpenseGroup = eg;
                            return et;
                        },
                        new { id = entity.ExpenseTypeId.Value },
                        UnitOfWork.Transaction
                    );

                entity.ExpenseType = result.FirstOrDefault();
            }

            entity.OrderExpense = GetOrderExpense(entity.Id);

            if (entity.BusinessOperationSettingId.HasValue)
            {
                entity.BusinessOperationSetting = await _businessOperationSettingRepository.GetAsync(entity.BusinessOperationSettingId.Value);
            }

            if (entity.ApprovedId.HasValue)
                entity.Approved = await _userRepository.GetAsync(entity.ApprovedId.Value);
            
            entity.AuctionRequestId = await UnitOfWork.Session.ExecuteScalarAsync<Guid?>(
                @"SELECT RequestId FROM AuctionPayments WHERE CashOrderId = @id",
                new { id = entity.Id },
                UnitOfWork.Transaction);

            return entity;
        }
        
        public async Task<IEnumerable<CashOrder>> GetMultipleByActionIds(List<int> contractActionIds)
        {
            if (contractActionIds == null || !contractActionIds.Any())
            {
                return Enumerable.Empty<CashOrder>();
            }

            var parameters = new { ContractActionIds = contractActionIds };
            var cashOrderSqlQuery = @"
                SELECT *
                FROM CashOrders
                WHERE DeleteDate IS NULL
                  AND ContractActionId IN @ContractActionIds";
    
            var cashOrders = await UnitOfWork.Session
                .QueryAsync<CashOrder>(cashOrderSqlQuery, parameters, UnitOfWork.Transaction);
    
            return cashOrders;
        }
        
        public async Task<IEnumerable<CashOrder>> GetMultipleByIds(IEnumerable<int> cashOrderIds)
        {
            if (cashOrderIds.IsNullOrEmpty())
            {
                return Enumerable.Empty<CashOrder>();
            }

            var parameters = new { CashOrderIds = cashOrderIds };
            var cashOrderSqlQuery = @"
                SELECT *
                FROM CashOrders
                WHERE DeleteDate IS NULL
                  AND Id IN @CashOrderIds";
    
            var cashOrders = await UnitOfWork.Session
                .QueryAsync<CashOrder>(cashOrderSqlQuery, parameters, UnitOfWork.Transaction);
    
            return cashOrders;
        }

        public CashOrder Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var contractId = query?.Val<int?>("ContractId");
            var actionType = query?.Val<ContractActionType?>("ActionType");
            if (!contractId.HasValue)
                throw new ArgumentNullException(nameof(contractId));
            if (!actionType.HasValue)
                throw new ArgumentNullException(nameof(actionType));

            var entity = UnitOfWork.Session.Query<CashOrder, Client, User, Account, Account, User, Organization, CashOrder>(@"
SELECT TOP 1 co.*, c.*, u.*, da.*, ca.*, a.*, o.*
FROM ContractActionRows car
JOIN ContractActions cact ON car.ActionId = cact.Id AND cact.ContractId = @contractId AND cact.ActionType = @actionType AND cact.DeleteDate IS NULL
JOIN CashOrders co ON car.OrderId = co.Id
LEFT JOIN Clients c ON co.ClientId = c.Id
LEFT JOIN Users u ON co.UserId = u.Id
JOIN Accounts da ON co.DebitAccountId = da.Id
JOIN Accounts ca ON co.CreditAccountId = ca.Id
JOIN Users a ON co.AuthorId = a.Id
JOIN Members m ON a.Id = m.Id
JOIN Organizations o ON m.OrganizationId = o.Id",
                (co, c, u, da, ca, a, o) =>
                {
                    co.Client = c;
                    co.User = u;
                    co.DebitAccount = da;
                    co.CreditAccount = ca;
                    co.Author = a;
                    a.Organization = o;
                    co.Language = GetCashOrderPrintLanguageByOrderId(co.Id).Result;
                    co.OrderExpense = GetOrderExpense(co.Id);
                    return co;
                },
                new { contractId, actionType }, UnitOfWork.Transaction).FirstOrDefault();

            return entity;
        }

        public List<CashOrder> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var orderType = query?.Val<OrderType?>("OrderType");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var clientId = query?.Val<int?>("ClientId");
            var userId = query?.Val<int?>("UserId");
            var accountId = query?.Val<int?>("AccountId");
            var orderNumber = query?.Val<int?>("OrderNumber");
            var isDelete = query?.Val<bool?>("IsDelete");
            var ownerId = query?.Val<int?>("OwnerId");
            var ApproveStatus = (int?)query?.Val<OrderStatus?>("ApproveStatus");
            var businessOperationSettingId = query?.Val<int?>("BusinessOperationSettingId");
            var businessOperationId = query?.Val<int?>("BusinessOperationId");
            var contractId = query?.Val<int?>("ContractId");
            var accountPlanId = query?.Val<int?>("AccountPlanId");

            var pre = isDelete.HasValue && isDelete.Value ? "WHERE co.DeleteDate IS NOT NULL" : "WHERE co.DeleteDate IS NULL";
            pre += orderType.HasValue ? " AND co.OrderType = @orderType" : string.Empty;
            pre += beginDate.HasValue ? " AND co.OrderDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND co.OrderDate <= @endDate" : string.Empty;
            pre += clientId.HasValue ? " AND co.ClientId = @clientId" : string.Empty;
            pre += userId.HasValue ? " AND co.UserId = @userId" : string.Empty;
            pre += accountId.HasValue
                ? " AND (co.DebitAccountId = @accountId OR co.CreditAccountId = @accountId)"
                : string.Empty;
            pre += orderNumber.HasValue ? " AND co.OrderNumber = @orderNumber" : string.Empty;
            pre += ownerId.HasValue && ownerId != -1 ? " AND co.OwnerId = @ownerId" : string.Empty;
            pre += ApproveStatus.HasValue ? $" AND ApproveStatus = {ApproveStatus}" : string.Empty;
            pre += businessOperationSettingId.HasValue ? " AND co.BusinessOperationSettingId = @businessOperationSettingId" : string.Empty;
            pre += businessOperationId.HasValue ? " AND co.BusinessOperationId = @businessOperationId" : string.Empty;
            pre += contractId.HasValue ? " AND co.ContractId = @contractId" : string.Empty;
            pre += accountPlanId.HasValue
                ? " AND (da.AccountPlanId = @accountPlanId OR ca.AccountPlanId = @accountPlanId)"
                : string.Empty;

            string conditionText = "";
            string likeText = "";
            if (!string.IsNullOrWhiteSpace(listQuery.Filter))
            {
                conditionText = @"LEFT JOIN Clients c with(index = idx_Clients_FullNameBin_IdentityNumberBin_MobilePhoneBin) ON co.ClientId = c.Id      
	                            LEFT JOIN Users u ON co.UserId = u.Id";
                likeText = @"AND 
	                        (
		                        co.OrderNumberBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%' 
		                        OR 
		                        c.FullNameBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%' 
		                        OR 
		                        u.Fullname LIKE N'%' + @filter + N'%'
	                        )";
            }
            var page = listQuery.Page();
            var order = string.Empty;
            if (page != null)
            {
                order = listQuery.Order("co.OrderDate DESC", new Sort
                {
                    Name = "co.Id",
                    Direction = SortDirection.Desc
                });
            }

            var list = UnitOfWork.Session.Query<CashOrder, Client, User, Account, Account, Group, User, CashOrder>($@"
                        declare @filter2 NVARCHAR(4000) = upper(@filter);

                        ;WITH CashOrderPaged 
                        AS 
                        (
	                        SELECT co.Id      
	                        FROM CashOrders co  
	                        {conditionText}      
	                        LEFT JOIN Accounts da ON co.DebitAccountId = da.Id      
	                        LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id      
	                        {pre} 
	                         {likeText}
	                        {order} {page}
                        )  
                        SELECT co.*, op.Id AS TasOnlinePaymentId, uk.[Status] as UKassaStatus, c.*, u.*, da.*, ca.*, g.*, a.*  
                        FROM CashOrderPaged cop  
                        JOIN CashOrders co ON cop.Id = co.Id  
                        LEFT JOIN Clients c ON co.ClientId = c.Id  
                        LEFT JOIN Users u ON co.UserId = u.Id  
                        LEFT JOIN Accounts da ON co.DebitAccountId = da.Id  
                        LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id  
                        JOIN Groups g ON co.BranchId = g.Id  
                        JOIN Users a ON co.AuthorId = a.Id  
                        LEFT JOIN TasOnlinePayments op ON op.OrderId = co.Id  
                        LEFT JOIN UKassaRequests uk on uk.CashOrderId = co.Id",
            (co, c, u, da, ca, g, a) =>
            {
                co.Client = c;
                co.User = u;
                co.DebitAccount = da;
                co.CreditAccount = ca;
                co.Branch = g;
                co.Author = a;
                co.Language = GetCashOrderPrintLanguageByOrderId(co.Id).Result;
                co.OrderExpense = GetOrderExpense(co.Id);
                return co;
            },
            new
            {
                orderType,
                beginDate,
                endDate,
                clientId,
                userId,
                orderNumber,
                accountId,
                ownerId,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                businessOperationSettingId,
                businessOperationId,
                contractId,
                accountPlanId
            }, UnitOfWork.Transaction, commandTimeout: 240).ToList();

            foreach (var entity in list)
            {
                if (entity.ApprovedId.HasValue)
                    entity.Approved = _userRepository.Get(entity.ApprovedId.Value);
                if (entity.Client != null && entity.Client.LegalFormId > 0)
                    entity.Client.LegalForm = _clientLegalFormRepository.Get(entity.Client.LegalFormId);
            }

            return list;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var orderType = query?.Val<OrderType?>("OrderType");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var clientId = query?.Val<int?>("ClientId");
            var userId = query?.Val<int?>("UserId");
            var accountId = query?.Val<int?>("AccountId");
            var orderNumber = query?.Val<int?>("OrderNumber");
            var isDelete = query?.Val<bool?>("IsDelete");
            var ownerId = query?.Val<int?>("OwnerId");
            var ApproveStatus = (int?)query?.Val<OrderStatus?>("ApproveStatus");
            var businessOperationSettingId = query?.Val<int?>("BusinessOperationSettingId");
            var businessOperationId = query?.Val<int?>("BusinessOperationId");
            var contractId = query?.Val<int?>("ContractId");
            var accountPlanId = query?.Val<int?>("AccountPlanId");

            var pre = isDelete.HasValue && isDelete.Value ? "where co.DeleteDate IS NOT NULL" : "where co.DeleteDate IS NULL";
            pre += orderType.HasValue ? " AND co.OrderType = @orderType" : string.Empty;
            pre += beginDate.HasValue ? " AND co.OrderDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND co.OrderDate <= @endDate" : string.Empty;
            pre += clientId.HasValue ? " AND co.ClientId = @clientId" : string.Empty;
            pre += userId.HasValue ? " AND co.UserId = @userId" : string.Empty;
            pre += accountId.HasValue
                ? " AND (co.DebitAccountId = @accountId OR co.CreditAccountId = @accountId)"
                : string.Empty;
            pre += orderNumber.HasValue ? " AND co.OrderNumber = @orderNumber" : string.Empty;
            pre += ownerId.HasValue && ownerId != -1 ? " AND co.OwnerId = @ownerId" : string.Empty;
            pre += ApproveStatus.HasValue ? $" AND ApproveStatus = {ApproveStatus}" : string.Empty;
            pre += businessOperationSettingId.HasValue ? " AND co.BusinessOperationSettingId = @businessOperationSettingId" : string.Empty;
            pre += businessOperationId.HasValue ? " AND co.BusinessOperationId = @businessOperationId" : string.Empty;
            pre += contractId.HasValue ? " AND co.ContractId = @contractId" : string.Empty;
            pre += accountPlanId.HasValue
                ? " AND (da.AccountPlanId = @accountPlanId OR ca.AccountPlanId = @accountPlanId)"
                : string.Empty;


            string conditionText = "";
            string likeText = "";
            if (!string.IsNullOrWhiteSpace(listQuery.Filter))
            {
                conditionText = @"LEFT JOIN Clients c with(index = idx_Clients_FullNameBin_IdentityNumberBin_MobilePhoneBin) ON co.ClientId = c.Id      
	                            LEFT JOIN Users u ON co.UserId = u.Id";
                likeText = @"AND 
	                        (
		                        co.OrderNumberBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%' 
		                        OR 
		                        c.FullNameBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%' 
		                        OR 
		                        u.Fullname LIKE N'%' + @filter + N'%'
	                        )";
            }
            return UnitOfWork.Session.ExecuteScalar<int>($@"
declare @filter2 NVARCHAR(4000) = upper(@filter);

SELECT COUNT(co.Id)  
FROM CashOrders co  
{conditionText} 
LEFT JOIN Accounts da ON co.DebitAccountId = da.Id  
LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id  
{pre} 
{likeText}", new
            {
                orderType,
                beginDate,
                endDate,
                clientId,
                userId,
                orderNumber,
                accountId,
                ownerId,
                listQuery.Filter,
                businessOperationSettingId,
                businessOperationId,
                contractId,
                accountPlanId
            }, UnitOfWork.Transaction, commandTimeout: 240);
        }

        public List<CashOrdersOperationsReport> GetCashOrdersReport(int branchId, DateTime dateFrom, DateTime dateTo)
        {
            var cashOperations = UnitOfWork.Session.Query<CashOrdersOperationsReport>($@"
--Покупка
SELECT 0 as OperationType, ISNULL(SUM(ordercost), 0)  as OperationSum, count(*) as OperationsCount
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NULL
AND ubo.CheckOperationType = 0
UNION ALL
--Возврат покупки
SELECT 1 as OperationType, ISNULL(SUM(ordercost), 0) as OperationSum, count(*) as OperationsCount
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NOT NULL
AND ubo.CheckStornoOperationType = 1
UNION ALL
--Продажа
SELECT 2 as OperationType, ISNULL(SUM(ordercost), 0) as OperationSum, count(*) as OperationsCount
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NULL
AND ubo.CheckOperationType = 2
UNION ALL
--Возврат продажи
SELECT 3 as OperationType, ISNULL(SUM(ordercost), 0) as OperationSum, count(*) as OperationsCount
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NOT NULL
AND ubo.CheckStornoOperationType = 3",
            new { branchId = branchId, dateFrom = dateFrom, dateTo = dateTo }, UnitOfWork.Transaction).ToList();

            var payments = UnitOfWork.Session.Query<CashOrdersPayment>($@"
--Покупка
SELECT 0 as OperationType, 0 as PaymentType,  ISNULL(sum(case ubo.paymenttype when 0 then co.ordercost else 0 end), 0) as PaymentSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NULL
AND ubo.CheckOperationType = 0
UNION ALL
SELECT 0 as OperationType, 1 as PaymentType,  ISNULL(sum(case ubo.paymenttype when 1 then co.ordercost else 0 end), 0) as PaymentSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NULL
AND ubo.CheckOperationType = 0
UNION ALL
--Возврат покупки
SELECT 1 as OperationType, 0 as PaymentType,  ISNULL(sum(case ubo.paymenttype when 0 then co.ordercost else 0 end), 0) as PaymentSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NOT NULL
AND ubo.CheckStornoOperationType = 1
UNION ALL
SELECT 1 as OperationType, 1 as PaymentType,  ISNULL(sum(case ubo.paymenttype when 1 then co.ordercost else 0 end), 0) as PaymentSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NOT NULL
AND ubo.CheckStornoOperationType = 1
UNION ALL
--Продажа
SELECT 2 as OperationType, 0 as PaymentType,  ISNULL(sum(case ubo.paymenttype when 0 then co.ordercost else 0 end), 0) as PaymentSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NULL
AND ubo.CheckOperationType = 2
UNION ALL
SELECT 2 as OperationType, 1 as PaymentType,  ISNULL(sum(case ubo.paymenttype when 1 then co.ordercost else 0 end), 0) as PaymentSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NULL
AND ubo.CheckOperationType = 2
UNION ALL
--Возврат продажи
SELECT 3 as OperationType, 0 as PaymentType,  ISNULL(sum(case ubo.paymenttype when 0 then co.ordercost else 0 end), 0) as PaymentSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NOT NULL
AND ubo.CheckStornoOperationType = 3
UNION ALL
SELECT 3 as OperationType, 1 as PaymentType,  ISNULL(sum(case ubo.paymenttype when 1 then co.ordercost else 0 end), 0) as PaymentSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate < @dateTo AND co.OwnerId = @branchId AND co.ApproveStatus = 10
AND co.DeleteDate IS NULL
AND co.StornoId IS NOT NULL
AND ubo.CheckStornoOperationType = 3
",
            new { branchId = branchId, dateFrom = dateFrom, dateTo = dateTo }, UnitOfWork.Transaction).ToList();

            foreach (var cashOperation in cashOperations)
            {
                cashOperation.CashOrdersPayments = payments.Where(x => x.OperationType == cashOperation.OperationType).ToList();
            }
            return cashOperations;
        }

        public List<MoneyPlacementsReport> GetMoneyPlacementsReport(int branchId, DateTime dateFrom, DateTime dateTo)
        {
            return UnitOfWork.Session.Query<MoneyPlacementsReport>($@"
--Внесение
SELECT 0 as OperationType, count(*) as OperationsCount, sum(co.ordercost) as MoneyPlacementSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate <= @dateTo AND co.OwnerId = @branchId
AND co.DeleteDate IS NULL
AND ((ubo.CashOperationType = 0 AND co.StornoId IS NULL) OR (ubo.CashStornoOperationType = 0 AND co.StornoId IS NOT NULL))
AND co.ApproveStatus = 10
UNION ALL
--Изьятие
SELECT 1 as OperationType, count(*) as OperationsCount, sum(co.ordercost) as MoneyPlacementSum
FROM CashOrders co 
INNER JOIN UKassaBOSettings ubo on ubo.BusinessOperationSettingId = co.BusinessOperationSettingId
WHERE co.OrderDate > @dateFrom AND co.OrderDate <= @dateTo AND co.OwnerId = @branchId
AND co.DeleteDate IS NULL
AND ((ubo.CashOperationType = 1 AND co.StornoId IS NULL) OR (ubo.CashStornoOperationType = 1 AND co.StornoId IS NOT NULL))
AND co.ApproveStatus = 10
",
            new { branchId = branchId, dateFrom = dateFrom, dateTo = dateTo }, UnitOfWork.Transaction).ToList();
        }

        public List<CashOrder> GetAllOrdersByContractActionId(int contractActionId)
        {
            var entity = UnitOfWork.Session.Query<CashOrder, Account, Account, OrderExpense, CashOrder>(@"
SELECT *
FROM CashOrders co
LEFT JOIN Accounts da ON co.DebitAccountId = da.Id
LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id
LEFT JOIN OrdersExpenses oe ON oe.Id = co.Id
WHERE co.DeleteDate IS NULL
AND co.ContractActionId = @сontractActionId", (co, da, ca, oe) => { co.DebitAccount = da; co.CreditAccount = ca; co.OrderExpense = oe; return co; },
                new { сontractActionId = contractActionId }, UnitOfWork.Transaction).ToList();

            return entity;
        }

        public List<CashOrder> GetAllCashOrdersByContractActionId(int contractActionId)
        {
            var entities = UnitOfWork.Session.Query<CashOrder, User, Account, Account, User, User, Organization, CashOrder>(@"
SELECT co.*, u.*, da.*, ca.*, a.*, b.*, o.*
FROM CashOrders co
LEFT JOIN Users u ON co.UserId = u.Id
LEFT JOIN Accounts da ON co.DebitAccountId = da.Id
LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id
JOIN Users a ON co.AuthorId = a.Id
JOIN Users b ON co.ApprovedId = b.Id
JOIN Members m ON a.Id = m.Id
JOIN Organizations o ON m.OrganizationId = o.Id
WHERE co.DeleteDate IS NULL
AND co.ContractActionId = @сontractActionId
AND co.OrderType in (10,20,60)",
                (co, u, da, ca, a, b, o) =>
                {
                    co.User = u;
                    co.DebitAccount = da;
                    co.CreditAccount = ca;
                    co.Author = a;
                    co.Approved = b;
                    a.Organization = o;
                    co.Language = GetCashOrderPrintLanguageByOrderId(co.Id).Result;
                    co.OrderExpense = GetOrderExpense(co.Id);
                    return co;
                },
                new { сontractActionId = contractActionId }, UnitOfWork.Transaction).ToList();

            foreach (var entity in entities)
            {
                if (entity.ClientId.HasValue)
                    entity.Client = _clientRepository.GetOnlyClient(entity.ClientId.Value);
            }

            return entities;
        }

        public decimal GetAccountSettingDebitTurnsByActionIds(List<int> actionIds, string accountSettingCode)
        {
            var neededStatus = new List<OrderStatus>
            {
                OrderStatus.WaitingForApprove,
                OrderStatus.WaitingForConfirmation,
                OrderStatus.Confirmed
            };
            return UnitOfWork.Session.ExecuteScalar<decimal>($@"
SELECT ISNULL(SUM(IIF(co.StornoId IS NULL, co.OrderCost, -1 * co.OrderCost)),0)
  FROM CashOrders co JOIN Accounts account ON co.DebitAccountId = account.Id
  JOIN AccountSettings setting ON account.AccountSettingId = setting.Id 
WHERE co.DeleteDate IS NULL
  AND co.ContractActionId IN @actionIds
  AND setting.Code = @accountSettingCode
  AND co.ApproveStatus IN @neededStatus",
new { actionIds, accountSettingCode, neededStatus }, UnitOfWork.Transaction);
        }
        public decimal GetAccountSettingCreditTurnsByActionIds(List<int> actionIds, string accountSettingCode)
        {
            var neededStatus = new List<OrderStatus>
            {
                OrderStatus.WaitingForApprove,
                OrderStatus.WaitingForConfirmation,
                OrderStatus.Confirmed
            };
            return UnitOfWork.Session.ExecuteScalar<decimal>($@"
SELECT ISNULL(SUM(IIF(co.StornoId IS NULL, co.OrderCost, -1 * co.OrderCost)),0)
  FROM CashOrders co JOIN Accounts account ON co.CreditAccountId = account.Id
  JOIN AccountSettings setting ON account.AccountSettingId = setting.Id 
WHERE co.DeleteDate IS NULL
  AND co.ContractActionId IN @actionIds
  AND setting.Code = @accountSettingCode
  AND co.ApproveStatus IN @neededStatus",
new { actionIds, accountSettingCode, neededStatus }, UnitOfWork.Transaction);
        }

        public List<CashOrder> GetOnlyCashOrdersByBranchAndDate(int branchId, DateTime dateFrom, DateTime dateTo)
        {
            var entity = UnitOfWork.Session.Query<CashOrder, Account, Account, OrderExpense, CashOrder>(@"
SELECT *
FROM CashOrders co
LEFT JOIN OrdersExpenses oe ON oe.Id = co.Id
LEFT JOIN Accounts da ON co.DebitAccountId = da.Id
LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id
WHERE co.OwnerId = @branchId and co.OrderDate >= @dateFrom and co.OrderDate <= @dateTo
and co.OrderType in (10, 20, 60) and co.ApproveStatus = 10 and co.DeleteDate is null", (co, da, ca, oe) =>
            {
                co.DebitAccount = da;
                co.CreditAccount = ca;
                co.OrderExpense = oe;
                return co;
            },
                new { branchId = branchId, dateFrom = dateFrom, dateTo = dateTo }, UnitOfWork.Transaction).ToList();
            return entity;
        }

        public List<int> GetOrderIdsForFiscalCheck(int ContractActionId)
        {
            return UnitOfWork.Session.Query<int>(@"
            SELECT Id FROM CashOrders WHERE ContractActionId = @ContractActionId AND OrderType IN (10,20,60) AND BusinessOperationSettingId != 206", new { ContractActionId }, UnitOfWork.Transaction).ToList();
        }

        public List<int> GetNewOrderIdsForFiscalCheck(string ContractActionIds, DateTime orderDate)
        {
            return UnitOfWork.Session.Query<int>($@"
            SELECT Id FROM CashOrders WHERE ContractActionId in ({ContractActionIds}) and OrderType in (10,20,60) AND 
            OrderDate > @orderDate", new { orderDate }, UnitOfWork.Transaction).ToList();
        }

        public DateTime GetLastOrderDate(string ContractActionIds)
        {
            return UnitOfWork.Session.ExecuteScalar<DateTime>($@"            
            SELECT Max(OrderDate) FROM CashOrders WHERE ContractActionId in ({ContractActionIds})", UnitOfWork.Transaction);
        }

        public int? GetCashOrderByContractId(int contractId)
        {
            var id = UnitOfWork.Session.Query<int?>(@"
                SELECT TOP 1 cas.Id FROM Contracts co 
                JOIN CashOrders cas ON cas.ContractId = co.Id
                WHERE co.DeleteDate IS NULL
                  AND cas.ApproveStatus = 10 AND cas.DeleteDate IS NULL
                  AND cas.StornoId IS NULL
                  AND co.Id = @contractId
                  AND NOT EXISTS (SELECT * FROM CashOrders cc WHERE cc.ContractId = co.Id AND cc.StornoId = cas.Id AND cc.DeleteDate IS NULL AND cc.ApproveStatus = 10)",
                new { contractId }, UnitOfWork.Transaction).FirstOrDefault();

            return id;
        }

        public async Task<List<int>> GetOrderIdsForCashOrderPrintForms(int ContractActionId)
        {
            return UnitOfWork.Session.Query<int>(@"
            SELECT Id FROM CashOrders WHERE ContractActionId = @ContractActionId AND OrderType IN (10,20,60)", new { ContractActionId }, UnitOfWork.Transaction).ToList();
        }

        public async Task SetCashOrderPrintLanguage(int cashOrderId, int languageId, int authorId)
        {
            var createDate = DateTime.Now;
            UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO CashOrderPrintLanguages
(CashOrderId, LanguageId, CreateDate, AuthorId)
VALUES
(@CashOrderId, @LanguageId, @CreateDate, @AuthorId)", new { cashOrderId, languageId, createDate, authorId }, UnitOfWork.Transaction);
        }

        public async Task DeleteCashOrderPrintLanguageByOrderId(int cashOrderId)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE CashOrderPrintLanguages SET DeleteDate = dbo.GETASTANADATE() WHERE CashOrderId = @cashOrderId", new { cashOrderId }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<Language?> GetCashOrderPrintLanguageByOrderIdAsync(int cashOrderId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Language?>(@"
SELECT l.* 
FROM Languages l 
JOIN CashOrderPrintLanguages pl ON pl.LanguageId = l.Id 
WHERE pl.CashOrderId = @cashOrderId
AND l.DeleteDate IS NULL
AND pl.DeleteDate IS NULL", new { cashOrderId }, UnitOfWork.Transaction);
        }

        public async Task<Language?> GetCashOrderPrintLanguageByOrderId(int cashOrderId)
        {
            return UnitOfWork.Session.Query<Language?>(@"
SELECT l.* 
FROM Languages l 
JOIN CashOrderPrintLanguages pl ON pl.LanguageId = l.Id 
WHERE pl.CashOrderId = @cashOrderId
AND l.DeleteDate IS NULL
AND pl.DeleteDate IS NULL", new { cashOrderId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public CashOrder GetCashOrderByContractIdWithoutParams(int ContractId)
        {
            var entity = UnitOfWork.Session.Query<CashOrder>(@"SELECT c.*
  FROM CashOrders c
  JOIN BusinessOperationSettings bos on bos.Id = c.BusinessOperationSettingId
 WHERE bos.Code = 'CREDIT_CARD_OUT'
   AND c.ContractId = @ContractId", new { ContractId }, UnitOfWork.Transaction).FirstOrDefault();
            if (entity != null)
            {
                entity.OrderExpense = GetOrderExpense(entity.Id);
            }
            return entity;
        }

        /// <summary>
        /// Возвращает общую проведенную сумму ордеров договора по списку бизнес-операций за период
        /// </summary>
        /// <param name="contractId">Id договора</param>
        /// <param name="boOperationSettings">список с кодами бизнес-операций</param>
        /// <param name="startDate">дата начала периода</param>
        /// <param name="endDate">дата окончания периода</param>
        /// <returns></returns>
        public async Task<decimal> GetContractTotalOperationAmount(int contractId, List<string> boOperationSettings, DateTime startDate, DateTime endDate)
        {
            return await UnitOfWork.Session.ExecuteScalarAsync<decimal>(@"
SELECT ISNULL(SUM(IIF(StornoId IS NULL, OrderCost, -OrderCost)),0) 
FROM CashOrders co JOIN BusinessOperationSettings bos ON co.BusinessOperationSettingId = bos.id
 WHERE co.DeleteDate IS NULL 
   AND co.ContractId = @contractId 
   AND bos.Code IN @boOperationSettings
   AND co.OrderDate >= @startDate AND co.OrderDate < @endDate
   AND co.ApproveStatus = 10 ", new {contractId, boOperationSettings, startDate, endDate}, UnitOfWork.Transaction);
        }

        public async Task<DateTime> GetContractLastOperationDate(int contractId, List<string> boOperationSettings, DateTime tillDate)
        {
            return await UnitOfWork.Session.ExecuteScalarAsync<DateTime>(@"
SELECT MAX(OrderDate)
FROM CashOrders co JOIN BusinessOperationSettings bos ON co.BusinessOperationSettingId = bos.id
 WHERE co.DeleteDate IS NULL 
   AND co.ContractId = @contractId 
   AND bos.Code IN @boOperationSettings
   AND co.OrderDate < @tillDate
   AND co.ApproveStatus = 10
            ", new { contractId, boOperationSettings, tillDate}, UnitOfWork.Transaction);
        }
    }
}