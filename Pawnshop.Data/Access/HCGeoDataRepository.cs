using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using System;
using System.Collections.Generic;
using Dapper;
using Pawnshop.Core.Queries;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class HCGeoDataRepository : RepositoryBase, IRepository<HCGeoData>
    {
        public HCGeoDataRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public void Insert(HCGeoData entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                        INSERT INTO HCGeoData (HCActionHistoryId, GpsCoordinates, AddressText, CreateDate)
                        VALUES(@HCActionHistoryId, @GpsCoordinates, @AddressText, @CreateDate)
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(HCGeoData entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public HCGeoData Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<HCGeoData>(@"
                            SELECT * 
                              FROM HCGeoData
                              WHERE Id=@id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public async Task<HCGeoData> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<HCGeoData>(@"
                            SELECT * 
                              FROM HCGeoData
                              WHERE Id=@id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public List<HCGeoData> GetByHistoryId(int HCActionHistoryId)
        {
            return UnitOfWork.Session.Query<HCGeoData>(@"
                            SELECT * 
                              FROM HCGeoData
                              WHERE HCActionHistoryId=@HCActionHistoryId AND DeleteDate IS NULL", new { HCActionHistoryId }, UnitOfWork.Transaction).AsList();
        }

        public async Task<HCGeoData> GetByHistoryIdAsync(int HCActionHistoryId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<HCGeoData>(@"
                            SELECT * 
                              FROM HCGeoData
                              WHERE HCActionHistoryId=@HCActionHistoryId AND DeleteDate IS NULL", new { HCActionHistoryId }, UnitOfWork.Transaction);
        }

        public HCGeoData Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<HCGeoData> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}
