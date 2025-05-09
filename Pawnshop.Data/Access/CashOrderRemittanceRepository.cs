using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;

namespace Pawnshop.Data.Access
{
    public class CashOrderRemittanceRepository : RepositoryBase, ICashOrderRemittanceRepository
    {
        public CashOrderRemittanceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IDbTransaction BeginTransaction()
        {
            return base.BeginTransaction();
        }

        public async Task Insert(CashOrderRemittance cashOrderRemittance)
        {
            var parameters = new
            {
                CashOrderId = cashOrderRemittance.CashOrderId,
                RemittanceId = cashOrderRemittance.RemittanceId,
                CreateDate = DateTime.Now
            };

            var sqlQuery = @"
                INSERT INTO CashOrderRemittances (CashOrderId, RemittanceId, CreateDate)
                OUTPUT INSERTED.Id
                VALUES (@CashOrderId, @RemittanceId, @CreateDate)";

            cashOrderRemittance.Id = await UnitOfWork.Session.ExecuteScalarAsync<int>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task<CashOrderRemittance> GetById(int id)
        {
            var parameters = new { Id = id };

            var sqlQuery = @"
                SELECT *
                FROM CashOrderRemittances
                WHERE DeleteDate IS NULL
                  AND Id = @Id";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<CashOrderRemittance>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task<CashOrderRemittance> GetByCashOrderId(int cashOrderId)
        {
            var parameters = new { CashOrderId = cashOrderId };

            var sqlQuery = @"
                SELECT *
                FROM CashOrderRemittances
                WHERE DeleteDate IS NULL
                  AND CashOrderId = @CashOrderId";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<CashOrderRemittance>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task<CashOrderRemittance> GetByRemittanceId(int remittanceId)
        {
            var parameters = new { RemittanceId = remittanceId };

            var sqlQuery = @"
                SELECT *
                FROM CashOrderRemittances
                WHERE DeleteDate IS NULL
                  AND RemittanceId = @RemittanceId";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<CashOrderRemittance>(sqlQuery, parameters, UnitOfWork.Transaction);
        }
    }
}