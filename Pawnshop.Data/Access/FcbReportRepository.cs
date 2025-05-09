using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Kdn;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Access
{
    public class FcbReportRepository : RepositoryBase, IRepository<FCBReport>
    {
        public FcbReportRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public FCBReport Find(object query)
        {
            throw new NotImplementedException();
        }

        public FCBReport Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<FCBReport>(@"
            SELECT * FROM FCBReports WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public List<FCBReport> GetByClientId(int ClientId)
        {
            return UnitOfWork.Session.Query<FCBReport, User, FCBReport>(@"SELECT f.*,
       u.*
  FROM FCBReports f
  JOIN Users u ON u.Id = f.AuthorId
 WHERE ClientId = @ClientId;",
                (f, u) =>
                {
                    f.Author = u;
                    return f;
                },
                new { ClientId }, UnitOfWork.Transaction)
                .AsList();
        }

        public void Insert(FCBReport entity)
        {
            entity.Id = UnitOfWork.Session.Execute(@"
            IF (NOT EXISTS(SELECT * FROM FCBReports WHERE ClientId = @ClientId AND FolderName = @FolderName AND PdfFileLink = @PdfFileLink AND XmlFileLink = @XmlFileLink))
            BEGIN
                INSERT INTO FCBReports ( ClientId, FolderName, PdfFileLink, XmlFileLink, CreateDate, AuthorId, DeleteDate, ReportType )
                VALUES ( @ClientId, @FolderName, @PdfFileLink, @XmlFileLink, dbo.GETASTANADATE(), @AuthorId, null, @ReportType)
            END", entity, UnitOfWork.Transaction);
        }

        public List<FCBReport> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(FCBReport entity)
        {
            throw new NotImplementedException();
        }
    }
}
