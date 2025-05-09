using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.Contracts;
using Dapper;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ContractCreditLineAdditionalLimitsRepository : RepositoryBase
    {
        public ContractCreditLineAdditionalLimitsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<decimal> GetLimitBySum(decimal sum)
        {
            return await UnitOfWork.Session.QueryFirstAsync<decimal>(@"
                SELECT TOP 1 LimitPercent FROM ContractCreditLineAdditionalLimit
                WHERE LowCost <= @sum AND HighCost >= @sum
                ORDER BY Id desc",
                new { sum }, UnitOfWork.Transaction);
        }

        public async Task<List<ContractCreditLineAdditionalLimit>> List()
        {
            return UnitOfWork.Session.Query<ContractCreditLineAdditionalLimit>(@"
                SELECT * LimitPercent FROM ContractCreditLineAdditionalLimit", 
                UnitOfWork.Transaction).ToList();
        }

        public ContractCreditLineAdditionalLimit Get(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var id = query?.Val<int?>("Id");
            var lowCost = query?.Val<int?>("LowCost");
            var highCost = query?.Val<int?>("HighCost");
            var limitPercent = query?.Val<decimal>("LimitPercent");

            var condition = @"WHERE";

            condition += id != null ? " AND Id = @id" : "";
            condition += lowCost != null ? " AND LowCost = @lowCost" : "";
            condition += highCost != null ? " AND HighCost = @highCost" : "";
            condition += limitPercent != null ? " AND LimitPercent = @limitPercent" : string.Empty;

            return UnitOfWork.Session.Query<ContractCreditLineAdditionalLimit>($@"SELECT TOP 1 * FROM {condition}",
                new {id, lowCost, highCost, limitPercent}
                ).FirstOrDefault();
        }

        public void Insert(ContractCreditLineAdditionalLimit cclal)
        {
            UnitOfWork.Session.Query("INSERT INTO ContractCreditLineAdditionalLimit (LowCost, HighCost, LimitPercent) VALUE (@lowCost, @highCost, @limitPercent)",
                new {
                    lowCost = cclal.LowCost,
                    highCost = cclal.HighCost,
                    limitPercent = cclal.LimitPercent,
                });
        }

        public void Update(ContractCreditLineAdditionalLimit cclal)
        {
            throw new NotImplementedException();
        }
    }
}
