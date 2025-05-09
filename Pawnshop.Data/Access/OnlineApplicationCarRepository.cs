using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.OnlineApplications;
using System;
using System.Collections.Generic;
using Pawnshop.Core.Queries;
using System.Threading.Tasks;
using Dapper;

namespace Pawnshop.Data.Access
{
    public class OnlineApplicationCarRepository : RepositoryBase, IRepository<OnlineApplicationCar>
    {
        public OnlineApplicationCarRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public OnlineApplicationCar Find(object query)
        {
            throw new NotImplementedException();
        }

        public OnlineApplicationCar Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(OnlineApplicationCar entity)
        {
            using (var transaction = BeginTransaction())
            {
                InternalInsert(entity);
                transaction.Commit();
            }
        }

        public void InternalInsert(OnlineApplicationCar entity)
        {
            UnitOfWork.Session.Execute(@"
INSERT INTO OnlineApplicationCars ( Id, Mark, Model, ReleaseYear, TransportNumber, MotorNumber, BodyNumber, TechPassportNumber, TechPassportDate, Color, VehicleMarkId, VehicleModelId )
VALUES ( @Id, @Mark, @Model, @ReleaseYear, @TransportNumber, @MotorNumber, @BodyNumber, @TechPassportNumber, @TechPassportDate, @Color, @VehicleMarkId, @VehicleModelId )",
                entity, UnitOfWork.Transaction);
        }

        public List<OnlineApplicationCar> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(OnlineApplicationCar entity)
        {
            using (var transaction = BeginTransaction())
            {
                InternalUpdate(entity);
                transaction.Commit();
            }
        }

        public void InternalUpdate(OnlineApplicationCar entity)
        {
            UnitOfWork.Session.Execute(@"
UPDATE OnlineApplicationCars
   SET Mark = @Mark, Model = @Model, ReleaseYear = @ReleaseYear, TransportNumber = @TransportNumber, MotorNumber = @MotorNumber,
       BodyNumber = @BodyNumber, TechPassportNumber = @TechPassportNumber, TechPassportDate = @TechPassportDate, Color = @Color,
       VehicleMarkId = @VehicleMarkId, VehicleModelId = @VehicleModelId
 WHERE Id = @Id",
                entity, UnitOfWork.Transaction);
        }

        public async Task<OnlineApplicationCar> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<OnlineApplicationCar>(
                @"SELECT * FROM OnlineApplicationCars WHERE Id = @id",
                new { id }, UnitOfWork.Transaction);
        }
    }
}
