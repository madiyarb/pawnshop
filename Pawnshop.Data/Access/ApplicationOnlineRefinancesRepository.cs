using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ApplicationOnlineRefinances;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineRefinancesRepository : RepositoryBase
    {
        public ApplicationOnlineRefinancesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public async Task Insert(ApplicationOnlineRefinance refinance)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(refinance, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<List<ApplicationOnlineRefinance>> GetApplicationOnlineRefinancesByApplicationId(Guid applicationId)
        {
            var builder = new SqlBuilder();
            builder.Where("ApplicationOnlineId = @ApplicationOnlineId",
                new { ApplicationOnlineId = applicationId });
            builder.Where("DeleteDate IS NULL");
            var selector = builder.AddTemplate(@$"SELECT * FROM ApplicationOnlineRefinances /**where**/ ");
            return (await UnitOfWork.Session.QueryAsync<ApplicationOnlineRefinance>(selector.RawSql,
                selector.Parameters, UnitOfWork.Transaction)).ToList();
        }

        public async Task<List<ApplicationOnlineRefinance>> GetApplicationOnlineRefinancesByContractId(int contractId)
        {
            var builder = new SqlBuilder();
            builder.Where("ContractId = @ContractId",
                new { ContractId = contractId });
            builder.Where("DeleteDate IS NULL");
            var selector = builder.AddTemplate(@$"SELECT * FROM ApplicationOnlineRefinances /**where**/ ");
            return (await UnitOfWork.Session.QueryAsync<ApplicationOnlineRefinance>(selector.RawSql,
                selector.Parameters, UnitOfWork.Transaction)).ToList();
        }

        public async Task Update(ApplicationOnlineRefinance refinance)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.UpdateAsync(refinance, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}
