using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class BlackListReasonRepository : RepositoryBase, IRepository<BlackListReason>
    {
        public BlackListReasonRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(BlackListReason entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO BlackListReasons(Name, AllowNewContracts, ReasonType, IsDisplayed, MustHaveAddedFile, MustHaveRemovedFile, DeleteDate, AllowNewContractsWithDrive, AdditionNewContractWithDrive, PartialPaymentWithDrive)
                    VALUES(@Name, @AllowNewContracts, @ReasonType, @IsDisplayed, @MustHaveAddedFile, @MustHaveRemovedFile, @DeleteDate, @AllowNewContractsWithDrive, @AdditionNewContractWithDrive, @PartialPaymentWithDrive)
                    SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(BlackListReason entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE BlackListReasons SET Name = @Name,AllowNewContracts = @AllowNewContracts,ReasonType = @ReasonType,
                    IsDisplayed = @IsDisplayed,MustHaveAddedFile = @MustHaveAddedFile,MustHaveRemovedFile = @MustHaveRemovedFile, DeleteDate = @DeleteDate,
                    AllowNewContractsWithDrive = @AllowNewContractsWithDrive,
                    AdditionNewContractWithDrive = @AdditionNewContractWithDrive,
                    PartialPaymentWithDrive = @PartialPaymentWithDrive
                    WHERE Id = @id", entity,UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE BlackListReasons SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public int RelationCount(int reasonId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
                SELECT SUM(c)
                    FROM (
                    SELECT COUNT(*) as c
                        FROM VehiclesBlackList
                        WHERE ReasonId = @reasonId AND DeleteDate IS NULL
                UNION ALL
                    SELECT COUNT(*) as c
                        FROM ClientsBlackList 
                            WHERE ReasonId = @reasonId AND DeleteDate IS NULL
                            ) as t", new { reasonId});
        }

        public BlackListReason Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<BlackListReason>(@"
                SELECT * 
                FROM BlackListReasons
                WHERE Id = @id", new { id });
        }

        public BlackListReason Find(object query)
        {
            if (query == null) 
                throw new ArgumentNullException(nameof(query));

            var name = query?.Val<string>("Name");

            return UnitOfWork.Session.Query<BlackListReason>(@"
                    SELECT * FROM BlackListReasons
                    WHERE Name = @name",
                new { name }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<BlackListReason> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) 
                throw new ArgumentNullException(nameof(listQuery));

            var reasonType = query?.Val<ReasonType?>("ReasonType");
            var pre = "DeleteDate IS NULL";
            pre += reasonType.HasValue ? " AND ReasonType = @reasonType" : string.Empty;

            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<BlackListReason>($@"
                SELECT *
                FROM BlackListReasons
                {condition} {order} {page}", new
            {
                reasonType,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) 
                throw new ArgumentNullException(nameof(listQuery));

            var reasonType = query.Val<ReasonType?>("ReasonType");
            var pre = "DeleteDate IS NULL";
            pre += reasonType.HasValue ? " AND ReasonType = @reasonType" : string.Empty;

            var condition = listQuery.Like(pre, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM BlackListReasons
                {condition}", new
            {
                reasonType,
                listQuery.Filter
            });
        }

        public async Task<BlackListReason> GetAsync(string code)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<BlackListReason>(@"
                SELECT * 
                FROM BlackListReasons
                WHERE Code = @code", new { code });
        }
    }
}
