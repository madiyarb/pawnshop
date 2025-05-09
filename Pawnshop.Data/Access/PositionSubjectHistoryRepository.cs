using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core.Queries;
using Dapper;
using Pawnshop.Data.Models.Clients;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.Data.Access
{
    public class PositionSubjectHistoryRepository : RepositoryBase, IRepository<PositionSubjectHistory>
    {

        private readonly ClientRepository _clientRepository;

        public PositionSubjectHistoryRepository(IUnitOfWork unitOfWork, ClientRepository clientRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE PositionSubjectsHistory SET DeleteDate = dbo.GETASTANADATE()
                    WHERE Id = @Id",
                 new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public PositionSubjectHistory Find(object query)
        {
            throw new NotImplementedException();
        }

        public PositionSubjectHistory Get(int id)
        {
            return UnitOfWork.Session.Query<PositionSubjectHistory, LoanSubject, Client, PositionSubjectHistory>(@"
            SELECT ps.*, ls.*, cl.*
            FROM PositionSubjectsHistory ps
            JOIN LoanSubjects ls ON ls.Id = ps.SubjectId
            JOIN Clients c ON cl.Id = ls.ClientId
            WHERE ps.Id = @id",
            (ps, ls, cl) =>
            {
                cl.Addresses = _clientRepository.GetClientAddresses(cl.Id);
                cl.Documents = _clientRepository.GetClientDocumentsByClientId(cl.Id);
                ps.Subject = ls;
                ps.Client = cl;
                return ps;
            },
new { id }, UnitOfWork.Transaction).FirstOrDefault();

        }

        public void Insert(PositionSubjectHistory entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO PositionSubjectsHistory ( PositionId, SubjectId, ClientId, BeginDate, AuthorId, CreateDate)
                    VALUES ( @PositionId, @SubjectId, @ClientId, @BeginDate, @AuthorId, dbo.GETASTANADATE())
                    SELECT SCOPE_IDENTITY()",
                entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public List<PositionSubjectHistory> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));


            var positionId = query?.Val<int?>("PositionId");
            var subjectId = query?.Val<int?>("SubjectId");
            var clientId = query?.Val<int?>("ClientId");
            var beginDate = query?.Val<DateTime?>("BeginDate");

            var pre = " WHERE ps.DeleteDate IS NULL AND cl.DeleteDate IS NULL AND ls.DeleteDate IS NULL";

            if (positionId.HasValue) pre += " AND ps.PositionId = @positionId";
            if (subjectId.HasValue) pre += " AND ps.SubjectId = @subjectId";
            if (clientId.HasValue) pre += " AND ps.ClientId = @clientId";
            if (beginDate.HasValue) pre += " AND CAST(ps.BeginDate AS DATE) <= @beginDate";

            var condition = pre;


            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "ps.PositionId",
                Direction = SortDirection.Desc
            });

            var page = listQuery.Page();

            return UnitOfWork.Session.Query<PositionSubjectHistory, LoanSubject, PositionSubjectHistory>($@"
                SELECT ps.*, ls.*
                FROM PositionSubjectsHistory ps
                JOIN LoanSubjects ls ON ls.Id = ps.SubjectId
                JOIN Clients cl ON cl.Id = ps.ClientId
                {condition} {order}",
                (ps, ls) =>
                {
                    ps.Client = _clientRepository.Get(ps.ClientId);
                    ps.Subject = ls;
                    return ps;
                },
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter,
                    positionId,
                    subjectId,
                    clientId,
                }, UnitOfWork.Transaction).ToList();
        }

        public void Update(PositionSubjectHistory entity)
        {
            throw new NotImplementedException();
        }

        public async Task<PositionSubjectHistory> FindMainPledgerForPosition(int positionId, DateTime? beginDate = null)
        {

            if (!beginDate.HasValue)
                beginDate = DateTime.Now;

            var mainPledgerCode = Constants.MAIN_PLEDGER_CODE;

            var result = (await UnitOfWork.Session.QueryAsync<PositionSubjectHistory, LoanSubject, PositionSubjectHistory>($@"
                SELECT TOP 1 ps.*, ls.*
                FROM PositionSubjectsHistory ps
                JOIN LoanSubjects ls ON ls.Id = ps.SubjectId
                JOIN Clients cl ON cl.Id = ps.ClientId
                WHERE ps.PositionId = @positionId
                AND ps.BeginDate <= DATEADD(second, 1, @beginDate) -- added 1 second because of possible lost second for conversion from datetime to datetime2
                AND ls.Code = @mainPledgerCode
                ORDER BY ps.BeginDate DESC",
                (ps, ls) =>
                {
                    ps.Client = _clientRepository.Get(ps.ClientId);
                    ps.Subject = ls;
                    return ps;
                },
                new
                {
                    positionId,
                    beginDate,
                    mainPledgerCode,
                }, UnitOfWork.Transaction)).FirstOrDefault();

            return result;
        }
        
        public async Task<IList<PositionSubjectHistory>> ListCoPledgersFromHistory(int positionId, DateTime beginDate)
        {

            var coPledgerCode = Constants.PLEDGER_CODE;

            var result = (await UnitOfWork.Session.QueryAsync<PositionSubjectHistory, LoanSubject, PositionSubjectHistory>($@"
                SELECT ps.*, ls.*
                FROM PositionSubjectsHistory ps
                JOIN LoanSubjects ls ON ls.Id = ps.SubjectId
                JOIN Clients cl ON cl.Id = ps.ClientId
                WHERE ps.DeleteDate IS NULL AND ps.PositionId = @positionId
                AND ps.BeginDate = @beginDate
                AND ls.Code = @coPledgerCode",
                (ps, ls) =>
                {
                    ps.Client = _clientRepository.Get(ps.ClientId);
                    ps.Subject = ls;
                    return ps;
                },
                new
                {
                    positionId,
                    beginDate,
                    coPledgerCode,
                }, UnitOfWork.Transaction)).ToList();


            return result;
        }
    }
}
