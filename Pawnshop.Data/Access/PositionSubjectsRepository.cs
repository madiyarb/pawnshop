using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using Pawnshop.Core.Queries;
using Dapper;
using System.Linq;
using Pawnshop.Data.Models.Clients;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class PositionSubjectsRepository : RepositoryBase, IRepository<PositionSubject>
    {
        private readonly ClientRepository _clientRepository;
        public PositionSubjectsRepository(IUnitOfWork unitOfWork, ClientRepository clientRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
        }

        public void Insert(PositionSubject entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO PositionSubjects ( PositionId, SubjectId, ClientId, AuthorId, CreateDate)
                    VALUES ( @PositionId, @SubjectId, @ClientId, @AuthorId, dbo.GETASTANADATE())
                    SELECT SCOPE_IDENTITY()",
                entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(PositionSubject entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE PositionSubjects SET PositionId = @PositionId, SubjectId = @SubjectId, ClientId = @ClientId, AuthorId = @AuthorId, CreateDate = @CreateDate
                    WHERE Id = @Id",
entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public PositionSubject Get(int id)
        {
            return UnitOfWork.Session.Query<PositionSubject, LoanSubject, Client, PositionSubject>(@"
            SELECT ps.*, ls.*, cl.*
            FROM PositionSubjects ps
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

        public PositionSubject Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<PositionSubject> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));


            var positionId = query?.Val<int?>("PositionId");
            var subjectId = query?.Val<int?>("SubjectId");
            var clientId = query?.Val<int?>("ClientId");

            var pre = " WHERE ps.DeleteDate IS NULL AND cl.DeleteDate IS NULL AND ls.DeleteDate IS NULL";

            if (positionId.HasValue) pre += " AND ps.PositionId = @positionId";
            if (subjectId.HasValue) pre += " AND ps.SubjectId = @subjectId";
            if (clientId.HasValue) pre += " AND ps.ClientId = @clientId";

            var condition = pre;


            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "ps.PositionId",
                Direction = SortDirection.Desc
            });

            var page = listQuery.Page();

            return UnitOfWork.Session.Query<PositionSubject, LoanSubject, PositionSubject>($@"
                SELECT ps.*, ls.*
                FROM PositionSubjects ps
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

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE PositionSubjects SET DeleteDate = dbo.GETASTANADATE()
                    WHERE Id = @Id",
new {id}, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public List<PositionSubject> GetOnlyPositionSubjectForPosition(int positionId)
        {
            return UnitOfWork.Session.Query<PositionSubject>(@"
            SELECT ps.*
            FROM PositionSubjects ps
            WHERE ps.PositionId = @positionId
            AND ps.DeleteDate IS NULL",
            new { positionId }, UnitOfWork.Transaction).ToList();
        }

        public List<Client> GetCurrentPositionSubjects(int positionId)
        {
            return UnitOfWork.Session.Query<Client>(@"
            SELECT cl.*
            FROM PositionSubjects ps
            JOIN Clients cl ON ps.ClientId = cl.Id
            WHERE ps.PositionId = @positionId
            AND ps.DeleteDate IS NULL
            AND cl.DeleteDate IS NULL",
new { positionId }, UnitOfWork.Transaction).ToList();
        }
    }
}
