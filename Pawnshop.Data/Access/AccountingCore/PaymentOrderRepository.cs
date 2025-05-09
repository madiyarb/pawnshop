using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using PaymentOrder = Pawnshop.Data.Models.AccountingCore.PaymentOrder;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class PaymentOrderRepository : RepositoryBase, IRepository<PaymentOrder>
    {
        public PaymentOrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(PaymentOrder entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO PaymentOrders (SequenceNumber, AccountSettingId, NotOnScheduleDateAllowed, AuthorId, IsActive, CreateDate, PaymentOrderSchema  )
VALUES (@SequenceNumber, @AccountSettingId, @NotOnScheduleDateAllowed, @AuthorId, @IsActive, @CreateDate, @PaymentOrderSchema  )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(PaymentOrder entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE PaymentOrders
SET SequenceNumber = @SequenceNumber, AccountSettingId = @AccountSettingId, NotOnScheduleDateAllowed = @NotOnScheduleDateAllowed, IsActive = @IsActive, PaymentOrderSchema = @PaymentOrderSchema
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public PaymentOrder Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<PaymentOrder>(@"
SELECT *
FROM PaymentOrders
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public async Task<PaymentOrder> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PaymentOrder>(@"
SELECT *
FROM PaymentOrders
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public PaymentOrder Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<PaymentOrder> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var isActive = query?.Val<bool?>("IsActive");
            var notOnScheduleDateAllowed = query?.Val<bool?>("NotOnScheduleDateAllowed");
            
            var pre = "po.Id>0";
            
            pre += isActive.HasValue ? " AND po.IsActive = @isActive" : string.Empty;
            pre += notOnScheduleDateAllowed.HasValue ? " AND po.NotOnScheduleDateAllowed = @NotOnScheduleDateAllowed" : string.Empty;

            var condition = listQuery.Like(pre);

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "po.SequenceNumber",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<PaymentOrder>($@"
SELECT *
FROM PaymentOrders po
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                isActive,
                notOnScheduleDateAllowed,
            }, UnitOfWork.Transaction).AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var isActive = query?.Val<bool?>("IsActive");
            var notOnScheduleDateAllowed = query?.Val<bool?>("NotOnScheduleDateAllowed");

            var pre = "po.Id>0";

            pre += isActive.HasValue ? " AND po.IsActive = @isActive" : string.Empty;
            pre += notOnScheduleDateAllowed.HasValue ? " AND po.NotOnScheduleDateAllowed = @NotOnScheduleDateAllowed" : string.Empty;

            var condition = listQuery.Like(pre);

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM PaymentOrders po
{condition}", new
            {
                listQuery.Filter,
                isActive,
                notOnScheduleDateAllowed,
            }, UnitOfWork.Transaction);
        }

    }
}