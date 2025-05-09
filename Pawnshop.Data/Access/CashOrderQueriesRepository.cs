using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public sealed class CashOrderQueriesRepository : RepositoryBase
    {
        private readonly UserRepository _userRepository;
        private readonly ClientLegalFormRepository _clientLegalFormRepository;
        public CashOrderQueriesRepository(IUnitOfWork unitOfWork,
            UserRepository userRepository,
            ClientLegalFormRepository clientLegalFormRepository) : base(unitOfWork)
        {
            _userRepository = userRepository;
            _clientLegalFormRepository = clientLegalFormRepository;
        }

        public async Task<IEnumerable<CashOrder>> List(ListQuery listQuery, object query = null)
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

            var list = await UnitOfWork.Session.QueryAsync<CashOrder, Client, User, Account, Account, Group, User, CashOrder>($@"
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
            }, UnitOfWork.Transaction, commandTimeout: 240);

            foreach (var entity in list)
            {
                if (entity.ApprovedId.HasValue)
                    entity.Approved = await _userRepository.GetAsync(entity.ApprovedId.Value);
                if (entity.Client != null && entity.Client.LegalFormId > 0)
                    entity.Client.LegalForm = _clientLegalFormRepository.Get(entity.Client.LegalFormId);
                entity.Language = await GetCashOrderPrintLanguageByOrderId(entity.Id);
                entity.OrderExpense = await GetOrderExpense(entity.Id);
            }

            return list;
        }


        public async Task<int> Count(ListQuery listQuery, object query = null)
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
            return await UnitOfWork.Session.ExecuteScalarAsync<int>($@"
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

        private async Task<OrderExpense> GetOrderExpense(int id)
        {
            return (await UnitOfWork.Session
                .QueryAsync<OrderExpense, Group, Models.CashOrders.ExpenseArticleType, OrderExpense>(@"
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

    }
}
