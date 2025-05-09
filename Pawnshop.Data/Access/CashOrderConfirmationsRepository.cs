using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class CashOrderConfirmationsRepository : RepositoryBase, IRepository<CashOrderConfirmation>
    {
        public CashOrderConfirmationsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE CashOrderConfirmations
                    SET DeleteDate = dbo.GETASTANADATE()
                         WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public CashOrderConfirmation Find(object query)
        {
            throw new NotImplementedException();
        }

        public CashOrderConfirmation Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<CashOrderConfirmation>(@"
                SELECT Id, OrderId, ConfirmedUserId, ConfirmationDate, Note, DeleteDate
                    FROM CashOrderConfirmations
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public CashOrderConfirmation GetConfirmationsByOrderId(int OrderId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<CashOrderConfirmation>(@"
                SELECT Id, OrderId, ConfirmedUserId, ConfirmationDate, Note, DeleteDate
                    FROM CashOrderConfirmations
                        WHERE OrderId = @OrderId", new { OrderId }, UnitOfWork.Transaction);
        }

        public void Insert(CashOrderConfirmation entity)
        {
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
            INSERT INTO CashOrderConfirmations ( OrderId, ConfirmedUserId, ConfirmationDate, Note, DeleteDate )
                VALUES ( @OrderId, @ConfirmedUserId, GETDATE(), Note, null);
            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public List<CashOrderConfirmation> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<CashOrderConfirmation>(@"
                SELECT OrderId, ConfirmedUserId, ConfirmationDate, Note, DeleteDate
                    FROM CashOrderConfirmations", UnitOfWork.Transaction).ToList();
        }

        public void Update(CashOrderConfirmation entity)
        {
            throw new NotImplementedException();
        }
    }
}
