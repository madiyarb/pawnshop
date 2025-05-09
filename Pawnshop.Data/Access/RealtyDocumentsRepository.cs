using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class RealtyDocumentsRepository : RepositoryBase, IRepository<RealtyDocument>
    {

        public RealtyDocumentsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE RealtyDocuments SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void DeleteForRealty(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE RealtyDocuments SET DeleteDate = dbo.GETASTANADATE() WHERE RealtyId = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public RealtyDocument Find(object query)
        {
            throw new NotImplementedException();
        }

        public RealtyDocument Get(int id)
        {
            return UnitOfWork.Session.Query<RealtyDocument, ClientDocumentType, RealtyDocument>($@"
            SELECT rd.*, cdp.*
            FROM RealtyDocuments rd
            JOIN ClientDocumentTypes cdp ON cdp.Id = rd.DocumentTypeId
            WHERE rd.Id = @id",
            (rd, cdp) =>
            {
                rd.DocumentType = cdp;
                return rd;
            },
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<RealtyDocument> GetDocumentsForRealty(int realtyId)
        {
            return UnitOfWork.Session.Query<RealtyDocument, ClientDocumentType, RealtyDocument>($@"
            SELECT rd.*, cdp.*
            FROM RealtyDocuments rd
            JOIN ClientDocumentTypes cdp ON cdp.Id = rd.DocumentTypeId
            WHERE rd.RealtyId = @realtyId
            AND rd.DeleteDate IS NULL",
            (rd, cdp) =>
            {
                rd.DocumentType = cdp;
                return rd;
            },
            new { realtyId }, UnitOfWork.Transaction).ToList();
        }

        public void Insert(RealtyDocument entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO RealtyDocuments (RealtyId, DocumentTypeId, Number, Date, CreateDate, AuthorId ) VALUES ( @RealtyId, @DocumentTypeId, @Number, @Date, @CreateDate, @AuthorId )
                    SELECT SCOPE_IDENTITY()",
                    new
                    {
                        RealtyId = entity.RealtyId,
                        DocumentTypeId = entity.DocumentTypeId,
                        Number = entity.Number,
                        Date = entity.Date,
                        CreateDate = DateTime.Now,
                        AuthorId = entity.AuthorId,
                    }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(RealtyDocument entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE RealtyDocuments SET RealtyId = @RealtyId, DocumentTypeId = @DocumentTypeId, Number = @Number, Date = @Date, CreateDate = @CreateDate, AuthorId = @AuthorId WHERE Id = @Id",
                    new
                    {
                        Id = entity.Id,
                        RealtyId = entity.RealtyId,
                        DocumentTypeId = entity.DocumentTypeId,
                        Number = entity.Number,
                        Date = entity.Date,
                        CreateDate = entity.CreateDate,
                        AuthorId = entity.AuthorId,
                    }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<RealtyDocument> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "rd.Id",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<RealtyDocument>($@"
                SELECT *
                FROM RealtyAddress rd
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
    }
}
