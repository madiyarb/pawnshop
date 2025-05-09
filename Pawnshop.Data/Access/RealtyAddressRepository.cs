using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class RealtyAddressRepository : RepositoryBase, IRepository<RealtyAddress>
    {
        public RealtyAddressRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM RealtyAddress WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public RealtyAddress Find(object query)
        {
            throw new NotImplementedException();
        }

        public RealtyAddress Get(int id)
        {
            return UnitOfWork.Session.Query<RealtyAddress>($@"
            SELECT ra.*
            FROM RealtyAddress ra
            WHERE ra.Id = @id",
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<IEnumerable<RealtyAddress>> GetByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryAsync<RealtyAddress>($@"
            SELECT ra.*
            FROM RealtyAddress ra
            INNER JOIN ContractPositions cp ON cp.PositionId = ra.Id
            WHERE cp.ContractId = @contractId
            AND cp.DeleteDate IS NULL",
            new { contractId }, UnitOfWork.Transaction);
        }

        public void Insert(RealtyAddress entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO RealtyAddress (Id, AteId, GeonimId, BuildingNumber, BuildingAdditionalNumber, AppartmentNumber, FullPathRus, FullPathKaz ) VALUES (@Id, @AteId, @GeonimId, @BuildingNumber, @BuildingAdditionalNumber, @AppartmentNumber, @FullPathRus, @FullPathKaz )",
                    new
                    {
                        Id = entity.Id,
                        AteId = entity.AteId,
                        GeonimId = entity.GeonimId,
                        BuildingNumber = entity.BuildingNumber,
                        BuildingAdditionalNumber = entity.BuildingAdditionalNumber,
                        AppartmentNumber = entity.AppartmentNumber,
                        FullPathRus = entity.FullPathRus,
                        FullPathKaz = entity.FullPathKaz
                    }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(RealtyAddress entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE RealtyAddress SET AteId = @AteId, GeonimId = @GeonimId, BuildingNumber = @BuildingNumber, BuildingAdditionalNumber = @BuildingAdditionalNumber, AppartmentNumber = @AppartmentNumber, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz WHERE Id = @Id",
                    new
                    {
                        Id = entity.Id,
                        AteId = entity.AteId,
                        GeonimId = entity.GeonimId,
                        BuildingNumber = entity.BuildingNumber,
                        BuildingAdditionalNumber = entity.BuildingAdditionalNumber,
                        AppartmentNumber = entity.AppartmentNumber,
                        FullPathRus = entity.FullPathRus,
                        FullPathKaz = entity.FullPathKaz
                    }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<RealtyAddress> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "ra.Id",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<RealtyAddress>($@"
                SELECT *
                FROM RealtyAddress ra
                {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return 0;
        }
    }
}
