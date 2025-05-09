using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Data.Models.TMF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access.TMF
{
    public class TMFPaymentRepository : RepositoryBase, IRepository<TMFPayment>
    {
        public TMFPaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(TMFPayment entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO TMFPayments ( OrderId, Status, TmfDocumentId, TMFContractId )
                    VALUES ( @OrderId, @Status, @TmfDocumentId, @TMFContractId )
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(TMFPayment entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE TMFPayments
                        SET OrderId = @OrderId, Status = @Status, TmfDocumentId = @TmfDocumentId, TMFContractId = @TMFContractId
                            WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public TMFPayment Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public TMFPayment FindPaymentByOrderId(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<TMFPayment>(@"
                SELECT *
                    FROM TMFPayments
                        WHERE OrderId = @id", new { id }, UnitOfWork.Transaction);
        }

        public TMFPayment Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<TMFPayment>(@"
                SELECT *
                    FROM TMFPayments
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public List<TMFPayment> List(ListQuery listQuery, object query = null)
        {
            if (listQuery is null)
                throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var clientId = query?.Val<int?>("ClientId");
            var status = query?.Val<TMFPaymentStatus?>("Status");

            var pre = "co.DeleteDate IS NULL";

            pre += beginDate.HasValue ? " AND co.OrderDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND co.OrderDate <= @endDate" : string.Empty;
            pre += clientId.HasValue ? " AND co.ClientId = @clientId" : string.Empty;
            pre += status.HasValue ? " AND op.Status = @status" : string.Empty;

            var condition = listQuery.Like(pre, "op.Status");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "co.OrderDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<TMFPayment, CashOrder, Client, User, TMFPayment>($@"
                SELECT op.*, co.*, cl.*, u.*
                  FROM TMFPayments op
                  JOIN CashOrders co ON co.Id = op.OrderId
                  JOIN Clients cl ON cl.Id = co.ClientId
                  JOIN Users u ON u.Id = co.AuthorId
                {condition} {order} {page}",
                (op, co, cl, u) =>
                {
                    op.Order = co;
                    co.Client = cl;
                    co.Author = u;

                    return op;
                },
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter,
                    beginDate,
                    endDate,
                    status,
                    clientId
                }, UnitOfWork.Transaction).ToList();
        }
        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery is null)
                throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var clientId = query?.Val<int?>("ClientId");

            var pre = "co.DeleteDate IS NULL";

            pre += beginDate.HasValue ? " AND co.OrderDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND co.OrderDate <= @endDate" : string.Empty;
            pre += clientId.HasValue ? " AND co.ClientId = @clientId" : string.Empty;

            var condition = listQuery.Like(pre, "op.Status");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                  SELECT COUNT(*)
                    FROM TMFPayments op
                        JOIN CashOrders co ON co.Id = op.OrderId
                        JOIN Clients cl ON cl.Id = co.ClientId
                        JOIN Users u ON u.Id = co.AuthorId
                {condition}", new
            {
                listQuery.Filter,
                beginDate,
                endDate,
                clientId
            }, UnitOfWork.Transaction);
        }
    }
}
