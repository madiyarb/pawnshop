using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Data.Models.Dictionaries.Address;

namespace Pawnshop.Data.Access
{
    public class ContractProfileRepository : RepositoryBase, IRepository<ContractProfile>
    {
        public ContractProfileRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractProfile entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ContractProfiles(ContractId, BusinessTypeId, NewEmploymentNumberPlanned, IsStartingBorrower, 
                                                    CreateDate, AuthorId, NewEmploymentNumberActual, ATEId)
                        VALUES(@ContractId, @BusinessTypeId, @NewEmploymentNumberPlanned, @IsStartingBorrower, 
                                                    @CreateDate, @AuthorId, @NewEmploymentNumberActual, @ATEId)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ContractProfile entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractProfiles SET ContractId = @ContractId, BusinessTypeId = @BusinessTypeId, NewEmploymentNumberPlanned = @NewEmploymentNumberPlanned,
                                                IsStartingBorrower = @IsStartingBorrower, CreateDate = @CreateDate, AuthorId = @AuthorId, 
                                                    NewEmploymentNumberActual = @NewEmploymentNumberActual, ATEId = @ATEId
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractProfile Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractProfile>(@"
                SELECT * 
                    FROM ContractProfiles
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ContractProfile Find(object query)
        {
            var contractId = query?.Val<int?>("ContractId");

            var condition = "WHERE cp.DeleteDate IS NULL";

            condition += contractId.HasValue ? " AND cp.ContractId = @contractId" : string.Empty;

            return UnitOfWork.Session.Query<ContractProfile, AddressATE, ContractProfile>($@"
                SELECT cp.*, a.* 
                    FROM ContractProfiles cp
                    JOIN AddressATEs a ON a.Id = cp.ATEId
                        {condition}",
                (cp, a) =>
                {
                    cp.ATE = a;
                    return cp;
                }, new { contractId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ContractProfiles SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractProfile> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "ContractId");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractProfile>($@"
                SELECT * FROM ContractProfiles
                    {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                },
            UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";
            var condition = listQuery.Like(pre, "ContractId");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM ContractProfiles
                        {condition}",
                new
                {
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }
    }
}