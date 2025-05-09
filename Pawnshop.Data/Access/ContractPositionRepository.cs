using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Cars.Views;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ContractPositionRepository : RepositoryBase
    {
        public ContractPositionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public async Task<IEnumerable<ContractPosition>> GetContractPositionByContractId(int contractId)
        {
            var builder = new SqlBuilder();
            builder.Select("ContractPositions.*");
            builder.Where("ContractPositions.ContractId = @contractId",
                new { contractId });
            builder.Where("ContractPositions.DeleteDate IS NULL");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ContractPositions /**where**/ ");
            return UnitOfWork.Session.Query<ContractPosition>(builderTemplate.RawSql, builderTemplate.Parameters);
        }


        public async Task<IEnumerable<OnlineCarInfoView>> GetCarOnlineInfo(List<int> contractIds)
        {
            SqlBuilder builder = new SqlBuilder();

            #region Select
            builder.Select("ContractPositions.ContractId AS ContractId");
            builder.Select("Cars.Model AS Model");
            builder.Select("Cars.TransportNumber AS TransportNumber");
            builder.Select("Cars.BodyNumber AS BodyNumber");
            builder.Select("ContractPositions.EstimatedCost AS EstimatedCost");
            builder.Select("Cars.ReleaseYear AS IssueYear");
            builder.Select("Cars.ParkingStatusId AS ParkingStatus");
            builder.Select("CASE WHEN ContractPositions.CategoryId = 1 THEN 1 ELSE 0 END AS WithRightToDrive");

            #endregion


            #region Join

            builder.LeftJoin(@"Positions ON ContractPositions.PositionId = Positions.Id");
            builder.LeftJoin(@"Cars ON Cars.Id = Positions.Id ");

            #endregion

            #region Where

            builder.Where("ContractPositions.ContractId IN @contractIds", new { contractIds = contractIds });
            builder.Where("Positions.CollateralType = 20");

            #endregion


            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM ContractPositions 
            /**leftjoin**/ /**where**/ /**orderby**/ ");

            return await UnitOfWork.Session.QueryAsync<OnlineCarInfoView>(selector.RawSql, selector.Parameters);
        }


        public async Task<IEnumerable<RealtyInfoOnlineVIew>> GetRealtiesOnlineInfo(List<int> contractIds)
        {
            SqlBuilder builder = new SqlBuilder();

            #region Select
            builder.Select("ContractPositions.ContractId AS ContractId");
            builder.Select("Realties.RealtyTypeId AS RealtyType");
            builder.Select("Realties.Rca");
            builder.Select("Realties.CadastralNumber");
            builder.Select("ContractPositions.EstimatedCost");
            builder.Select("Realties.Year AS YearOfConstruction");

            #endregion


            #region Join

            builder.LeftJoin(@"Positions ON ContractPositions.PositionId = Positions.Id");
            builder.LeftJoin(@"Realties ON Realties.Id = ContractPositions.PositionId");

            #endregion

            #region Where

            builder.Where("ContractPositions.ContractId IN @contractIds", new { contractIds = contractIds });
            builder.Where("Positions.CollateralType = 60");

            #endregion


            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM ContractPositions 
            /**leftjoin**/ /**where**/ /**orderby**/ ");

            return await UnitOfWork.Session.QueryAsync<RealtyInfoOnlineVIew>(selector.RawSql, selector.Parameters);
        }
    }
}
