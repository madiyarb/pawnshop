using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Transfers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class TransferRepository : RepositoryBase, IRepository<Transfer>
    {
        public TransferRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<Transfer> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("Transfers.DeleteDate IS NULL",
                "PoolNumber", "PoolDate", "Status", "FullName");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "PoolDate",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Transfer, User, Transfer>($@"
                SELECT *
                  FROM Transfers
                  JOIN Users ON Users.Id = Transfers.UserId
                {condition} {order} {page}",
                            (t, u) =>
                            {
                                t.User = u;
                                return t;
                            }, new
                            {
                                listQuery.Page?.Offset,
                                listQuery.Page?.Limit,
                                listQuery.Filter
                            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("Transfers.DeleteDate IS NULL",
                "PoolNumber", "PoolDate", "Status", "FullName");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                  FROM Transfers
                  JOIN Users ON Users.Id = Transfers.UserId
                {condition}", new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public void Insert(Transfer entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
                    INSERT INTO Transfers
                              ( PoolNumber, CreateDate, PoolDate, DeleteDate, Status, UserId )
                    VALUES    ( @PoolNumber, @CreateDate, @PoolDate, @DeleteDate, @Status, @UserId )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Transfer entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Transfers
                    SET PoolNumber=@PoolNumber, PoolDate=@PoolDate, Status=@Status
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Transfers 
                    SET DeleteDate = dbo.GETASTANADATE()
                    WHERE Id = @Id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Transfer Get(int id)
        {
            throw new NotImplementedException();
        }

        public Transfer Find(object query)
        {
            throw new NotImplementedException();
        }
    }
}