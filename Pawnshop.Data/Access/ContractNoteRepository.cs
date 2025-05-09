using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class ContractNoteRepository : RepositoryBase, IRepository<ContractNote>
    {
        public ContractNoteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractNote entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ContractNotes ( ContractId, NoteDate, Note, AuthorId )
VALUES ( @ContractId, @NoteDate, @Note, @AuthorId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ContractNote entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ContractNotes
SET ContractId = @ContractId, NoteDate = @NoteDate, Note = @Note, AuthorId = @AuthorId
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM ContractNotes WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractNote Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractNote>(@"
SELECT *
FROM ContractNotes
WHERE Id = @id", new { id });
        }

        public ContractNote Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<ContractNote> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var contractId = query?.Val<int?>("ContractId");
            var clientId = query?.Val<int?>("ClientId");

            if (!contractId.HasValue && !clientId.HasValue) throw new ArgumentNullException($"{nameof(contractId)}, {nameof(clientId)}");

            var pre = contractId.HasValue
                ? "ContractNotes.ContractId = @contractId"
                : "Contracts.ClientId = @clientId";

            var condition = listQuery.Like(pre, "ContractNotes.Note");

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "ContractNotes.NoteDate",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractNote, User, ContractNote>($@"
SELECT ContractNotes.*, Users.*
FROM ContractNotes
JOIN Contracts ON ContractNotes.ContractId = Contracts.Id
JOIN Users ON ContractNotes.AuthorId = Users.Id
{condition} {order} {page}",
            (cn, u) =>
            {
                cn.Author = u;
                return cn;
            },
            new
            {
                contractId,
                clientId,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var contractId = query?.Val<int?>("ContractId");
            var clientId = query?.Val<int?>("ClientId");

            if (!contractId.HasValue && !clientId.HasValue) throw new ArgumentNullException($"{nameof(contractId)}, {nameof(clientId)}");

            var pre = contractId.HasValue
                ? "ContractNotes.ContractId = @contractId"
                : "Contracts.ClientId = @clientId";

            var condition = listQuery.Like(pre, "ContractNotes.Note");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM ContractNotes
JOIN Contracts ON ContractNotes.ContractId = Contracts.Id
{condition}", new
            {
                contractId,
                clientId,
                listQuery.Filter
            });
        }
    }
}