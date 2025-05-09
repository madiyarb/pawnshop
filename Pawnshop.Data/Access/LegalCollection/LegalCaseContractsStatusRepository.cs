using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Views;
using Pawnshop.Data.Models.LegalCollection.Entities;

namespace Pawnshop.Data.Access.LegalCollection
{
    public class LegalCaseContractsStatusRepository : RepositoryBase, ILegalCaseContractsStatusRepository
    {
        public LegalCaseContractsStatusRepository(IUnitOfWork unitOfWork) : base(unitOfWork) {}

        public async Task<LegalCaseContractsStatus?> GetByLegalCaseIdAsync(int legalCaseId)
        {
            var parameters = new { LegalCaseId = legalCaseId, IsActive = true };
            var sqlQuery = @"
                SELECT * FROM LegalCaseContractsStatus
                WHERE IsActive = @IsActive
                  AND DeleteDay IS NULL
                  AND LegalCaseId = @LegalCaseId";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<LegalCaseContractsStatus?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public LegalCaseContractsStatus GetByLegalCaseId(int legalCaseId)
        {
            var parameters = new { LegalCaseId = legalCaseId, IsActive = true };
            var sqlQuery = @"
                SELECT * FROM LegalCaseContractsStatus
                WHERE IsActive = @IsActive
                  AND DeleteDay IS NULL
                  AND LegalCaseId = @LegalCaseId";

            var result = UnitOfWork.Session
                .QueryFirstOrDefault<LegalCaseContractsStatus?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public async Task<LegalCaseContractsStatus?> GetActiveLegalCaseByContractIdAsync(int contractId)
        {
            var parameters = new { ContractId = contractId, IsActive = true };
            var sqlQuery = @"
                SELECT * FROM LegalCaseContractsStatus
                WHERE IsActive = @IsActive
                  AND ContractId = @ContractId";
            
            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<LegalCaseContractsStatus?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public LegalCaseContractsStatus GetActiveLegalCaseByContractId(int contractId)
        {
            var parameters = new { ContractId = contractId, IsActive = true };
            var sqlQuery = @"
                SELECT * FROM LegalCaseContractsStatus
                WHERE IsActive = @IsActive
                  AND ContractId = @ContractId";
            
            var result = UnitOfWork.Session
                .QueryFirstOrDefault<LegalCaseContractsStatus?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public async Task<LegalCaseContractsStatus?> GetNotActiveLegalCaseByContractIdAsync(int contractId)
        {
            var parameters = new { ContractId = contractId, IsActive = false };
            var sqlQuery = @"
                SELECT * FROM LegalCaseContractsStatus
                WHERE IsActive = @IsActive
                  AND ContractId = @ContractId
                  ORDER BY id DESC";
            
            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<LegalCaseContractsStatus?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public LegalCaseContractsStatus GetNotActiveLegalCaseByContractId(int contractId)
        {
            var parameters = new { ContractId = contractId, IsActive = false };
            var sqlQuery = @"
                SELECT * FROM LegalCaseContractsStatus
                WHERE IsActive = @IsActive
                  AND ContractId = @ContractId
                  ORDER BY id DESC";
            
            var result = UnitOfWork.Session
                .QueryFirstOrDefault<LegalCaseContractsStatus?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public async Task CloseAsync(int legalCaseId)
        {
            var parameters = new { LegalCaseId = legalCaseId, IsActive = false };
           
            var sqlQuery = @"
                    UPDATE LegalCaseContractsStatus
                    SET IsActive = @IsActive
                    WHERE LegalCaseId = @LegalCaseId";
            
            await UnitOfWork.Session.ExecuteAsync(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public void Close(int legalCaseId)
        {
            var parameters = new { LegalCaseId = legalCaseId, IsActive = false };
           
            var sqlQuery = @"
                    UPDATE LegalCaseContractsStatus
                    SET IsActive = @IsActive
                    WHERE LegalCaseId = @LegalCaseId";
            
            UnitOfWork.Session.Execute(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task ChangeActivityAsync(int legalCaseId, bool active)
        {
            var parameters = new { LegalCaseId = legalCaseId, IsActive = active };
            var sqlQuery = @"
                UPDATE LegalCaseContractsStatus
                SET IsActive = @IsActive
                WHERE LegalCaseId = @LegalCaseId";
            
            await UnitOfWork.Session.ExecuteAsync(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public void ChangeActivity(int legalCaseId, bool active)
        {
            var parameters = new { LegalCaseId = legalCaseId, IsActive = active };
            var sqlQuery = @"
                UPDATE LegalCaseContractsStatus
                SET IsActive = @IsActive
                WHERE LegalCaseId = @LegalCaseId";
            
            UnitOfWork.Session.Execute(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task InsertAsync(int contractId, int legalCaseId, bool isActive = true)
        {
            var parameters = new { ContractId = contractId, LegalCaseId = legalCaseId, IsActive = isActive };
            var sqlQuery = @"
                INSERT INTO LegalCaseContractsStatus (ContractId, LegalCaseId, IsActive)
                VALUES (@ContractId, @LegalCaseId, @IsActive)";
            
            await UnitOfWork.Session.ExecuteAsync(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public void Insert(int contractId, int legalCaseId, bool isActive = true)
        {
            var parameters = new { ContractId = contractId, LegalCaseId = legalCaseId, IsActive = isActive };
            var sqlQuery = @"
                INSERT INTO LegalCaseContractsStatus (ContractId, LegalCaseId, IsActive)
                VALUES (@ContractId, @LegalCaseId, @IsActive)";
            
            UnitOfWork.Session.Execute(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public LegalCaseContractsStatus? GetContractLegalCase(int contractId, bool isActive = true)
        {
            var parameters = new { ContractId = contractId, IsActive = isActive };

            var sqlQuery = @"
                SELECT * FROM LegalCaseContractsStatus
                WHERE ContractId = @ContractId
                  AND IsActive = @IsActive";

            var result = UnitOfWork.Session
                .QueryFirstOrDefault<LegalCaseContractsStatus?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public async Task<LegalCaseContractsStatus> GetContractLegalCaseAsync(int contractId, bool isActive = true)
        {
            var parameters = new { ContractId = contractId, IsActive = isActive };

            var sqlQuery = @"
                SELECT * FROM LegalCaseContractsStatus
                WHERE ContractId = @ContractId
                  AND IsActive = @IsActive";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<LegalCaseContractsStatus?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public async Task<Contract?> GetContractByLegalCaseIdAsync(int legalCaseId)
        {
            var parameters = new { LegalCaseId = legalCaseId };

            var sqlQuery = @"
                SELECT C.*
                FROM Contracts C
                         JOIN LegalCaseContractsStatus LC on LC.ContractId = C.Id
                WHERE C.DeleteDate IS NULL
                  AND LC.LegalCaseId = @LegalCaseId";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Contract?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }
        
        public Contract? GetContractByLegalCaseId(int legalCaseId)
        {
            var parameters = new { LegalCaseId = legalCaseId };

            var sqlQuery = @"
                SELECT C.*
                FROM Contracts C
                         JOIN LegalCaseContractsStatus LC on LC.ContractId = C.Id
                WHERE C.DeleteDate IS NULL
                  AND LC.LegalCaseId = @LegalCaseId";
            
            var result = UnitOfWork.Session
                .QueryFirstOrDefault<Contract?>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            return result;
        }

        public async Task<IEnumerable<LegalCollectionInfoView>> GeLegalCollectionStatusesOnlineView(List<int> contractIds)
        {
            SqlBuilder builder = new SqlBuilder();

            #region Select

            builder.Select("LegalCaseContractsStatus.ContractId");
            builder.Select("LegalCaseContractsStatus.IsActive AS InLegalCollection");

            #endregion

            #region Where

            builder.Where("LegalCaseContractsStatus.ContractId IN @contractIds", new { contractIds = contractIds });
            builder.Where("LegalCaseContractsStatus.ISActive = 1");
            builder.Where("LegalCaseContractsStatus.DeleteDay IS NULL");
            #endregion


            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM LegalCaseContractsStatus 
             /**where**/ /**orderby**/ ");

            return await UnitOfWork.Session.QueryAsync<LegalCollectionInfoView>(selector.RawSql, selector.Parameters);
        }
    }
}