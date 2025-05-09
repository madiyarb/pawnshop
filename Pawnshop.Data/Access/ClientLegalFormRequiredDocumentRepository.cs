using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class ClientLegalFormRequiredDocumentRepository : RepositoryBase, IRepository<ClientLegalFormRequiredDocument>
    {
        public ClientLegalFormRequiredDocumentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientLegalFormRequiredDocument entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ClientLegalFormRequiredDocuments(LegalFormId, DocumentTypeId, CreateDate, AuthorId)
                    VALUES(@LegalFormId, @DocumentTypeId, @CreateDate, @AuthorId)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientLegalFormRequiredDocument entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientLegalFormRequiredDocuments SET LegalFormId = @LegalFormId, DocumentTypeId = @DocumentTypeId, CreateDate = @CreateDate, AuthorId = @AuthorId
                    WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientLegalFormRequiredDocuments SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ClientLegalFormRequiredDocument Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientLegalFormRequiredDocument>(@"
                SELECT * 
                FROM ClientLegalFormRequiredDocuments
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ClientLegalFormRequiredDocument Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return UnitOfWork.Session.Query<ClientLegalFormRequiredDocument>(@"
                    SELECT * FROM ClientLegalFormRequiredDocuments", UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ClientLegalFormRequiredDocument> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<ClientLegalFormRequiredDocument>(@"SELECT * FROM ClientLegalFormRequiredDocuments", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ClientLegalFormRequiredDocuments", UnitOfWork.Transaction);
        }

        public List<ClientLegalFormRequiredDocument> ListByLegalForm(int legalFormId, bool isResident)
        {
            return UnitOfWork.Session.Query<ClientLegalFormRequiredDocument, ClientLegalForm, ClientDocumentType, ClientLegalFormRequiredDocument>($@"
SELECT clr.*, clf.*, cdt.* 
FROM ClientLegalFormRequiredDocuments clr 
  JOIN ClientLegalForms    clf ON clr.LegalFormId = clf.Id
  JOIN ClientDocumentTypes cdt ON clr.DocumentTypeId = cdt.Id
WHERE clr.IsMandatory = 1
AND clf.Id=@legalFormId
AND clr.IsResident = @isResident",
                (clr, clf, cdt) =>
                {
                    clr.DocumentType = cdt;
                    clr.LegalForm = clf;                    
                    return clr;
                },
                new
                {
                    legalFormId,
                    isResident
                }, UnitOfWork.Transaction, commandTimeout: 1800).ToList();
        }
    }
}