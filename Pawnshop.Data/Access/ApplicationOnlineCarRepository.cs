using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access.ApplicationOnlineHistoryLogger;
using Pawnshop.Data.Helpers;
using Pawnshop.Data.Models.ApplicationOnlineCarLogItems;
using Pawnshop.Data.Models.ApplicationsOnlineCar;
using Pawnshop.Data.Models.PrintFormInfo;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineCarRepository : RepositoryBase
    {
        private readonly IApplicationOnlineHistoryLoggerService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationOnlineCarRepository(IUnitOfWork unitOfWork,
            IApplicationOnlineHistoryLoggerService service,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationOnlineCar Get(Guid id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlineCar>(@"Select * from ApplicationOnlineCars where id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public async Task Insert(ApplicationOnlineCar car)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(car, UnitOfWork.Transaction);
                transaction.Commit();
            }

            _service.LogApplicationOnlineCarData(new ApplicationOnlineCarLogData(car),
                _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
        }

        public void Update(ApplicationOnlineCar car)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"
                UPDATE ApplicationOnlineCars SET 
                Mark= @Mark, 
                Model= @Model, 
                ReleaseYear= @ReleaseYear, 
                TransportNumber= @TransportNumber, 
                MotorNumber= @MotorNumber, 
                BodyNumber= @BodyNumber, 
                TechPassportNumber= @TechPassportNumber, 
                TechPassportDate= @TechPassportDate, 
                Color= @Color, 
                VehicleMarkId= @VehicleMarkId, 
                VehicleModelId= @VehicleModelId, 
                TechPassportModel= @TechPassportModel, 
                Category= @Category, 
                TechCategory= @TechCategory, 
                OwnerRegionName= @OwnerRegionName, 
                OwnerRegionNameEn= @OwnerRegionNameEn, 
                Notes= @Notes,
                Liquidity = @Liquidity
                WHERE  Id= @Id
                SELECT SCOPE_IDENTITY()", car, UnitOfWork.Transaction);

                transaction.Commit();
            }
            _service.LogApplicationOnlineCarData(new ApplicationOnlineCarLogData(car),
                _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);

        }

        /// <summary>
        /// Найти существующую машину в базу по VINCODE(BodyNumber)
        /// </summary>
        public async Task<ApplicationOnlineCar?> GetExistedCarByBodyNumber(string bodyNumber)
        {
            var builder = new SqlBuilder();
            builder.Select("Id = NEWID()");
            builder.Select("Cars.Mark");
            builder.Select("Cars.Model");
            builder.Select("Cars.ReleaseYear");
            builder.Select("Cars.TransportNumber");
            builder.Select("Cars.MotorNumber");
            builder.Select("Cars.BodyNumber");
            builder.Select("Cars.TechPassportNumber");
            builder.Select("Cars.Color");
            builder.Select("Cars.VehicleMarkId");
            builder.Select("Cars.VehicleModelId");
            builder.Select("Cars.TechPassportNumber");
            builder.Select("Cars.Id AS CarId");
            builder.Select("cars.TechPassportDate AS TechPassportDate");

            builder.Where("Cars.BodyNumber = @bodyNumber", new { bodyNumber });
            var selector = builder.AddTemplate($"Select /**select**/ from CARS /**where**/ order by CarId desc");
            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlineCar>(selector.RawSql,
                selector.Parameters);
        }


        public async Task<PrintFormOpenCreditLineQuestionnaireCollateralInfo> GetClientCollateralInfoForPrintForm(Guid id)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationOnlineCars.Mark");
            builder.Select("ApplicationOnlineCars.Model");
            builder.Select("ApplicationOnlineCars.ReleaseYear");
            builder.Select("ApplicationOnlineCars.TransportNumber");
            builder.Select("ApplicationOnlineCars.BodyNumber");
            builder.Select("ApplicationOnlineCars.Color");

            builder.Where("ApplicationOnlineCars.id = @id",
                new { id });

            var builderTemplate = builder.AddTemplate("Select /**select**/ from ApplicationOnlineCars /**leftjoin**/ /**where**/ /**orderby**/  ");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireCollateralInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<ApplicationOnlineCar> GetByApplicationId(Guid applicationId)
        {
            return await UnitOfWork.Session.QuerySingleOrDefaultAsync<ApplicationOnlineCar>(@"SELECT aoc.*
  FROM ApplicationOnlineCars aoc
  JOIN ApplicationsOnline ao ON ao.ApplicationOnlinePositionId = aoc.Id
 WHERE ao.Id = @applicationId",
                new { applicationId }, UnitOfWork.Transaction);
        }
    }
}
