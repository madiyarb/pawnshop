using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CardCashOutTransaction;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.PayOperations;

namespace Pawnshop.Data.Access
{
    public class PayOperationRepository : RepositoryBase, IRepository<PayOperation>
    {
        private readonly ClientRepository _clientRepository;
        private readonly ContractActionRepository _contractActionRepository;

        public PayOperationRepository(IUnitOfWork unitOfWork, ClientRepository clientRepository, ContractActionRepository contractActionRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
            _contractActionRepository = contractActionRepository;
        }

        public void Insert(PayOperation entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO PayOperations ( Date, PayTypeId, RequisiteTypeId, RequisiteId, Name, Note, ClientId, ContractId, ActionId, BranchId, AuthorId, CreateDate, Number, ExecuteDate, Status )
VALUES ( @Date, @PayTypeId, @RequisiteTypeId, @RequisiteId, @Name, @Note, @ClientId, @ContractId, @ActionId, @BranchId, @AuthorId, @CreateDate, @Number, @ExecuteDate, @Status )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                if (entity.Files.Count > 0)
                {
                    foreach (var file in entity.Files)
                    {
                        var filerow = new
                        {
                            PayOperationId = entity.Id,
                            FileRowId = file.Id
                        };

                        UnitOfWork.Session.Execute(@"
INSERT INTO PayOperationFileRows
( PayOperationId, FileRowId )
VALUES ( @PayOperationId, @FileRowId )", filerow, UnitOfWork.Transaction);
                    }
                }

                transaction.Commit();
            }
        }

        public void Update(PayOperation entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE PayOperations
SET PayTypeId = @PayTypeId, Name = @Name, Note = @Note, ClientId = @ClientId, ContractId = @ContractId, ActionId = @ActionId, BranchId = @BranchId,
ExecuteDate = @ExecuteDate, Status = @Status, Number = @Number, Date = @Date
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE PayOperations SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public PayOperation Get(int id)
        {
            var result = UnitOfWork.Session.Query<PayOperation, Group, User, PayType, ClientRequisite,  ContractAction, PayOperation >(@"
SELECT po.*, g.*, m.OrganizationId, u.*, pt.*, cr.*, ca.*
FROM PayOperations po
LEFT JOIN Groups g ON g.Id = po.BranchId
LEFT JOIN Members m ON m.Id = g.Id
LEFT JOIN Users u ON u.Id = po.AuthorId
LEFT JOIN PayTypes pt ON pt.Id = po.PayTypeId
LEFT JOIN ClientRequisites cr ON cr.Id = po.RequisiteId
LEFT JOIN ContractActions ca ON ca.Id = po.ActionId
WHERE po.Id = @id",
            (po, g, u, pt, cr, ca) => {
                po.Branch = g;
                po.Author = u;
                if(po.ClientId.HasValue)
                    po.Client = _clientRepository.Get(po.ClientId.Value);
                po.PayType = pt;
                po.Requisite = cr;
                po.Action = ca;
                return po;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (result.ActionId.HasValue)
                result.Action = _contractActionRepository.Get(result.ActionId.Value);

            GetMoreAboutOperation(result);

            return result;
        }

        public PayOperation Find(object query)
        {
            throw new System.NotImplementedException();
        }

        private void GetMoreAboutOperation(PayOperation operation)
        {
            operation.Files = UnitOfWork.Session.Query<FileRow>(@"
                SELECT fr.* FROM FileRows fr
                JOIN PayOperationFileRows cfr ON cfr.FileRowId=fr.Id
                WHERE cfr.PayOperationId= @id", new { id = operation.Id }, UnitOfWork.Transaction).ToList();

            operation.Orders = UnitOfWork.Session.Query<CashOrder, User, Account, Account, User, Organization, CashOrder>(@"
SELECT co.*, u.*, da.*, ca.*, a.*, o.*
FROM CashOrders co
LEFT JOIN Users u ON co.UserId = u.Id
LEFT JOIN Accounts da ON co.DebitAccountId = da.Id
LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id
JOIN Users a ON co.AuthorId = a.Id
JOIN Members m ON a.Id = m.Id
JOIN Organizations o ON m.OrganizationId = o.Id
WHERE co.OperationId = @id",
                (co, u, da, ca, a, o) =>
                {
                    if(co.ClientId.HasValue)
                        co.Client = _clientRepository.Get(co.ClientId.Value);
                    co.User = u;
                    co.DebitAccount = da;
                    co.CreditAccount = ca;
                    co.Author = a;
                    a.Organization = o;
                    return co;
                },
                new { id = operation.Id }, UnitOfWork.Transaction).ToList();

            operation.Actions = UnitOfWork.Session.Query<PayOperationAction, User, PayOperationAction>(@"
                SELECT a.*, u.* FROM PayOperationActions a
                LEFT JOIN Users u ON u.Id = a.AuthorId
                WHERE a.OperationId= @id", (a, u) => {
                a.Author = u;
                return a;
            }, new { id = operation.Id }, UnitOfWork.Transaction).ToList();

            if (operation.RequisiteId.HasValue && operation.Requisite!=null)
            {
                operation.Requisite.Bank = UnitOfWork.Session.Query<Client>(@"
                SELECT * FROM Clients WHERE Id = @id", new { id = operation.Requisite.BankId }, UnitOfWork.Transaction).FirstOrDefault();
            }
        }

        public List<PayOperation> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var pre = "po.DeleteDate IS NULL AND pt.OperationCode != 'CREDIT_CARD'";
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var branchId = query?.Val<int?>("BranchId");
            var branchIds = query?.Val<List<int>>("BranchIds");
            var organizationId = query?.Val<int?>("OrganizationId");
            var organizationIds = query?.Val<List<int>>("OrganizationIds");
            var clientId = query?.Val<int?>("ClientId");
            var payTypeId = query?.Val<int?>("PayTypeId");
            var requisiteTypeId = query?.Val<int?>("RequisiteTypeId");
            var requisiteId = query?.Val<int?>("RequisiteId");
            var actionType = query?.Val<ContractActionType?>("ActionType");
            var status = query?.Val<PayOperationStatus?>("Status");
            var contractId = query?.Val<int?>("ContractId");

            pre += branchId.HasValue ? " AND po.BranchId = @branchId" : string.Empty;
            pre += branchIds != null && branchIds.Count > 0 ? " AND po.BranchId IN @branchIds" : string.Empty;
            pre += organizationId.HasValue ? " AND m.OrganizationId = @organizationId" : string.Empty;
            pre += organizationIds != null && organizationIds.Count > 0 ? " AND m.OrganizationId IN @organizationIds" : string.Empty;
            pre += beginDate.HasValue ? " AND po.[Date] >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND po.[Date] <= @endDate" : string.Empty;
            pre += clientId.HasValue ? " AND po.ClientId = @clientId" : string.Empty;
            pre += payTypeId.HasValue ? " AND po.PayTypeId = @payTypeId" : string.Empty;
            pre += requisiteTypeId.HasValue ? " AND po.RequisiteTypeId = @requisiteTypeId" : string.Empty;
            pre += requisiteId.HasValue ? " AND po.RequisiteId = @requisiteId" : string.Empty;
            pre += actionType.HasValue ? " AND ca.ActionType = @actionType" : string.Empty;
            pre += status.HasValue ? " AND po.Status = @status" : string.Empty;
            pre += contractId.HasValue ? " AND po.ContractId = @contractId" : string.Empty;

            var condition = listQuery.Like(pre, "po.Number");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "po.CreateDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            var result = UnitOfWork.Session.Query<PayOperation, Group, User, Client, PayType, ClientRequisite, ContractAction, PayOperation>($@"
SELECT po.*, g.*, m.OrganizationId, u.*, c.*, pt.*, cr.*, ca.*
FROM PayOperations po
LEFT JOIN Groups g ON g.Id = po.BranchId
LEFT JOIN Members m ON m.Id = g.Id
LEFT JOIN Users u ON u.Id = po.AuthorId
LEFT JOIN Clients c ON c.Id = po.ClientId
LEFT JOIN PayTypes pt ON pt.Id = po.PayTypeId
LEFT JOIN ClientRequisites cr ON cr.Id = po.RequisiteId
LEFT JOIN ContractActions ca ON ca.Id = po.ActionId
{condition} {order} {page}",
            (po, g, u, c, pt, cr, ca) => {
                po.Branch = g;
                po.Author = u;
                po.Client = c;
                po.PayType = pt;
                po.Requisite = cr;
                po.Action = ca;
                return po;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                beginDate,
                endDate,
                branchId,
                branchIds,
                organizationId,
                organizationIds,
                clientId,
                payTypeId,
                requisiteTypeId,
                requisiteId,
                actionType,
                status,
                contractId
            }, UnitOfWork.Transaction).ToList();

            result.ForEach(operation =>
            {
                GetMoreAboutOperation(operation);
            });

            return result.ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "po.DeleteDate IS NULL";
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var branchId = query?.Val<int?>("BranchId");
            var branchIds = query?.Val<List<int>>("BranchIds");
            var organizationId = query?.Val<int?>("OrganizationId");
            var organizationIds = query?.Val<List<int>>("OrganizationIds");
            var clientId = query?.Val<int?>("ClientId");
            var payTypeId = query?.Val<int?>("PayTypeId");
            var requisiteTypeId = query?.Val<int?>("RequisiteTypeId");
            var requisiteId = query?.Val<int?>("RequisiteId");
            var actionType = query?.Val<ContractActionType?>("ActionType");
            var status = query?.Val<PayOperationStatus?>("Status");
            var contractId = query?.Val<int?>("ContractId");

            pre += branchId.HasValue ? " AND po.BranchId = @branchId" : string.Empty;
            pre += branchIds != null && branchIds.Count > 0 ? " AND po.BranchId IN @branchIds" : string.Empty;
            pre += organizationId.HasValue ? " AND m.OrganizationId = @organizationId" : string.Empty;
            pre += organizationIds != null && organizationIds.Count > 0 ? " AND m.OrganizationId IN @organizationIds" : string.Empty;
            pre += beginDate.HasValue ? " AND po.[Date] >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND po.[Date] <= @endDate" : string.Empty;
            pre += clientId.HasValue ? " AND po.ClientId = @clientId" : string.Empty;
            pre += payTypeId.HasValue ? " AND po.PayTypeId = @payTypeId" : string.Empty;
            pre += requisiteTypeId.HasValue ? " AND po.RequisiteTypeId = @requisiteTypeId" : string.Empty;
            pre += requisiteId.HasValue ? " AND po.RequisiteId = @requisiteId" : string.Empty;
            pre += actionType.HasValue ? " AND ca.ActionType = @actionType" : string.Empty;
            pre += status.HasValue ? " AND po.Status = @status" : string.Empty;
            pre += contractId.HasValue ? " AND po.ContractId = @contractId" : string.Empty;

            var condition = listQuery.Like(pre, "po.Number");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM PayOperations po
LEFT JOIN Groups g ON g.Id = po.BranchId
LEFT JOIN Members m ON m.Id = g.Id
LEFT JOIN Users u ON u.Id = po.AuthorId
LEFT JOIN Clients c ON c.Id = po.ClientId
LEFT JOIN PayTypes pt ON pt.Id = po.PayTypeId
LEFT JOIN ClientRequisites cr ON cr.Id = po.RequisiteId
LEFT JOIN ContractActions ca ON ca.Id = po.ActionId
{condition}", new
            {
                listQuery.Filter,
                beginDate,
                endDate,
                branchId,
                branchIds,
                organizationId,
                organizationIds,
                clientId,
                payTypeId,
                requisiteTypeId,
                requisiteId,
                actionType,
                status,
                contractId
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int modelId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
    SELECT COUNT(*) 
    FROM ContractActions
    WHERE PayOperationId = @modelId", new { modelId }, UnitOfWork.Transaction);
        }

        public PayOperation GetCreditCardCashOutPayOperationByContractId(int id)
        {
            return UnitOfWork.Session
                .Query<PayOperation>
                    (@"SELECT po.*
                      FROM CashOrders co
                      JOIN PayOperations po ON po.Id = co.OperationId
                      JOIN PayTypes pt ON pt.Id = po.PayTypeId
                     WHERE co.ContractId = @id
                       AND co.DeleteDate is null
                       AND po.DeleteDate is null
                       AND pt.OperationCode = 'CREDIT_CARD'
                     ORDER by Id DESC;", new { id },
                        UnitOfWork.Transaction).FirstOrDefault();
        }


        public PayOperation GetPayOperationByContractId(int id)
        {
            return UnitOfWork.Session
                .Query<PayOperation>
                (@"SELECT po.*
                      FROM CashOrders co
                      JOIN PayOperations po ON po.Id = co.OperationId
                      JOIN PayTypes pt ON pt.Id = po.PayTypeId
                     WHERE co.ContractId = @id
                       AND co.DeleteDate is null
                       AND po.DeleteDate is null
                     ORDER by Id DESC;", new { id },
                    UnitOfWork.Transaction).FirstOrDefault();
        }

        public PayOperation GetPayOperationByContractIdWithoutCashOrders(int id)
        {
            return UnitOfWork.Session
                .Query<PayOperation>
                (@"SELECT po.*
                     FROM PayOperations po 
                     JOIN PayTypes pt ON pt.Id = po.PayTypeId
                    WHERE po.ContractId = @id
                      AND po.DeleteDate is null
                    ORDER by Id DESC;", new { id },
                    UnitOfWork.Transaction).FirstOrDefault();
        }
    }
}