using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class PositionEstimatesRepository : RepositoryBase, IRepository<PositionEstimate>
    {
        public PositionEstimatesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE PositionEstimates SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public PositionEstimate Find(object query)
        {
            throw new NotImplementedException();
        }
       
        public PositionEstimate Get(int id)
        {
            return UnitOfWork.Session.Query<PositionEstimate, Client, PositionEstimate>($@"
            SELECT pe.*, cl.*
            FROM PositionEstimates pe
            JOIN Clients cl ON pe.CompanyId = cl.Id
            WHERE pe.Id = @id",
            (pe, cl) =>
            {
                pe.Company = cl;
                return pe;
            },
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Insert(PositionEstimate entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO PositionEstimates (PositionId, CompanyId, Date, Number, FileRowId, CreateDate, AuthorId ) VALUES (@PositionId, @CompanyId, @Date, @Number, @FileRowId, @CreateDate, @AuthorId )
                    SELECT SCOPE_IDENTITY()",
                    new
                    {
                        Id = entity.Id,
                        PositionId = entity.PositionId,
                        CompanyId = entity.CompanyId,
                        Date = entity.Date,
                        Number = entity.Number,
                        FileRowId = entity.FileRowId,
                        CreateDate = entity.CreateDate,
                        AuthorId = entity.AuthorId
                    }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(PositionEstimate entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE PositionEstimates SET PositionId = @PositionId, CompanyId = @CompanyId, Date = @Date, Number = @Number, FileRowId = @FileRowId, CreateDate = @CreateDate, AuthorId = @AuthorId WHERE Id = @Id",
                    new
                    {
                        Id = entity.Id,
                        PositionId = entity.PositionId,
                        CompanyId = entity.CompanyId,
                        Date = entity.Date,
                        Number = entity.Number,
                        FileRowId = entity.FileRowId,
                        CreateDate = entity.CreateDate,
                        AuthorId = entity.AuthorId
                    }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<PositionEstimate> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "pe.Id",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<PositionEstimate>($@"
                SELECT *
                FROM PositionEstimates pe
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

        public async Task<(PositionEstimate, int)> GetActualPositionEstimationInformation(int positionId)
        {

            var actualContractPosition = UnitOfWork.Session.Query<ContractPosition,PositionEstimate, Client, ContractPosition>(@"
SELECT TOP 1 cp.*, pe.*, cl.*
FROM ContractPositions cp
JOIN Contracts c ON c.Id = cp.ContractId 
JOIN PositionEstimates pe ON pe.Id = cp.EstimationId
JOIN Clients cl ON pe.CompanyId = cl.Id
WHERE cp.PositionId = @positionId
AND cp.DeleteDate IS NULL
AND c.DeleteDate IS NULL
AND c.Status IN (20,30,40, 50, 60)
AND pe.DeleteDate IS NULL
ORDER BY c.ContractDate",(cp, pe, cl) =>
            {
                pe.Company = cl;
                cp.PositionEstimate = pe;
                return cp;

            },
            new { positionId }).FirstOrDefault();

            if (actualContractPosition == null)
                return (new PositionEstimate(), 0);

            return (actualContractPosition.PositionEstimate, actualContractPosition.EstimatedCost);
        }

        public async Task<List<ContractPosition>> GetActualContractPositionsWithEstimateForPosition(int positionId)
        {
            var actualContractPositions = UnitOfWork.Session.Query<ContractPosition, PositionEstimate, Contract, Position, Client, ContractPosition>(@"
SELECT cp.*, pe.*, c.*, p.*, company.*
FROM ContractPositions cp
JOIN Contracts c ON c.Id = cp.ContractId 
JOIN PositionEstimates pe ON pe.Id = cp.EstimationId
JOIN Positions p ON cp.PositionId = p.Id
JOIN Clients company ON company.Id = pe.CompanyId
WHERE cp.PositionId = @positionId
AND cp.DeleteDate IS NULL
AND c.DeleteDate IS NULL
AND c.Status IN (20,30,40, 50, 60)
AND pe.DeleteDate IS NULL
ORDER BY c.ContractDate", (cp, pe, c, p, company) =>
            {
                pe.Company = company;
                cp.PositionEstimate = pe;
                cp.Contract = c;
                cp.Position = p;
                return cp;
            },
            new { positionId }, UnitOfWork.Transaction).ToList();

            return actualContractPositions;
        }
    }
}
