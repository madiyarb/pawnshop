using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class ParkingActionRepository : RepositoryBase, IRepository<ParkingAction>
    {
        public ParkingActionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ParkingAction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ParkingActions (ActionName, StatusBeforeId, StatusAfterId, CategoryId, CollateralType)
	                                    VALUES (@ActionName, @StatusBeforeId, @StatusAfterId, @CategoryId, @CollateralType)
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ParkingAction entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ParkingActions SET ActionName=@ActionName, StatusBeforeId=@StatusBeforeId, StatusAfterId=@StatusAfterId, CategoryId=@CategoryId, CollateralType=@CollateralType
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"UPDATE ParkingActions SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ParkingAction Find(object query)
        {
            throw new NotImplementedException();
        }

        public ParkingAction Get(int id)
        {
            return UnitOfWork.Session.Query<ParkingAction, ParkingStatus , ParkingStatus , Category, ParkingAction>(@"
                SELECT pa.*, psb.*, psa.*, c.*
                FROM ParkingActions pa
                    JOIN ParkingStatuses psb ON psb.Id=pa.StatusBeforeId
                    JOIN ParkingStatuses psa ON psa.Id=pa.StatusAfterId   
                    JOIN Categories c ON c.Id=pa.CategoryId
                WHERE pa.Id = @id",
                    (pa, psb, psa, c) =>
                    {
                        pa.StatusBefore = psb;
                        pa.StatusAfter = psa;
                        pa.Category = c;
                        return pa;
                    },
                    new { id },UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ParkingAction> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var statusBeforeId = query?.Val<int>("StatusBeforeId");
            var statusAfterId = query?.Val<int>("StatusAfterId");
            var actionName = query?.Val<string>("ActionName");
            var collateralType = query?.Val<CollateralType>("CollateralType");
            var categoryId = query?.Val<int>("CategoryId");

            string pre = statusBeforeId.HasValue ? " pa.DeleteDate IS NULL AND pa.StatusBeforeId = @statusBeforeId " : " pa.DeleteDate IS NULL ";
            pre += statusAfterId.HasValue ? " AND pa.StatusAfterId = @statusAfterId " : "";
            pre += !String.IsNullOrEmpty(actionName) ? " AND pa.ActionName = @actionName " : "";
            pre += collateralType.HasValue ? " AND pa.CollateralType = @collateralType " : "";
            pre += categoryId.HasValue ? " AND pa.CategoryId = @categoryId " : "";

            var condition = listQuery.Like(pre, "pa.ActionName", "pa.StatusBeforeId", "pa.StatusAfterId", "pa.CategoryId", "pa.CollateralType");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "pa.ActionName",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ParkingAction, ParkingStatus, ParkingStatus, Category, ParkingAction>($@"
                SELECT pa.*, psb.*, psa.*, c.*
                FROM ParkingActions pa
                    JOIN ParkingStatuses psb ON psb.Id=pa.StatusBeforeId
                    JOIN ParkingStatuses psa ON psa.Id=pa.StatusAfterId   
                    JOIN Categories c ON c.Id=pa.CategoryId
                {condition} {order} {page}", (pa, psb, psa, c) =>
            {
                pa.StatusBefore = psb;
                pa.StatusAfterId = psb.Id;
                pa.StatusAfter = psa;
                pa.StatusAfterId = psa.Id;
                pa.Category = c;
                pa.CategoryId = c.Id;
                return pa;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                statusBeforeId,
                statusAfterId,
                actionName,
                collateralType,
                categoryId
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("DeleteDate IS NULL", "ActionName", "StatusBeforeId", "StatusAfterId", "CategoryId", "CollateralType");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                  FROM ParkingActions
                {condition}", new
                    {
                        listQuery.Filter
                    });
        }
    }
}
