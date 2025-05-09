using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Positions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class PositionEstimateHistoryRepository : RepositoryBase, IRepository<PositionEstimateHistory>
    {

        public PositionEstimateHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        { 
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE PositionEstimateHistoryRepository SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public PositionEstimateHistory Find(object query)
        {
            throw new NotImplementedException();
        }

        public PositionEstimateHistory Get(int id)
        {
            return UnitOfWork.Session.Query<PositionEstimateHistory>($@"
            SELECT *
            FROM PositionEstimateHistory
            WHERE Id = @id",
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Insert(PositionEstimateHistory entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO PositionEstimateHistory (PositionId, CompanyId, Date, Number, EstimatedCost, CollateralCost, FileRowId, BeginDate, CreateDate, AuthorId ) VALUES (@PositionId, @CompanyId, @Date, @Number, @EstimatedCost, @CollateralCost, @FileRowId, @BeginDate, @CreateDate, @AuthorId )
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public List<PositionEstimateHistory> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(PositionEstimateHistory entity)
        {
            throw new NotImplementedException();
        }

        public List<PositionEstimateHistory> ListEstimationHistoryForPosition(int positionId)
        {

            return UnitOfWork.Session.Query<PositionEstimateHistory, Client, PositionEstimateHistory>(@"
SELECT *
FROM PositionEstimateHistory peh
JOIN Clients cl ON peh.CompanyId = cl.Id
WHERE peh.PositionId = @positionId
AND peh.DeleteDate IS NULL
AND cl.DeleteDate IS NULL
ORDER BY peh.Date DESC",(peh, cl) =>
            {
                peh.Company = cl;
                return peh;
            },new { positionId }, UnitOfWork.Transaction).ToList();
        }
    }
}
