using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class ClientSignersAllowedDocumentTypeRepository : RepositoryBase, IRepository<ClientSignersAllowedDocumentType>
    {
        public ClientSignersAllowedDocumentTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientSignersAllowedDocumentType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ClientSignersAllowedDocumentTypes(CompanyLegalFormId, DocumentTypeId, CreateDate, AuthorId,IsMandatory)
                        VALUES(@CompanyLegalFormId, @DocumentTypeId, @CreateDate, @AuthorId, @IsMandatory)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientSignersAllowedDocumentType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientSignersAllowedDocumentTypes SET CompanyLegalFormId = @CompanyLegalFormId, DocumentTypeId = @DocumentTypeId,
                        CreateDate = @CreateDate, AuthorId = @AuthorId, IsMandatory = @IsMandatory
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientSignersAllowedDocumentType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientSignersAllowedDocumentType>(@"
                SELECT * 
                    FROM ClientSignersAllowedDocumentTypes
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ClientSignersAllowedDocumentType Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientSignersAllowedDocumentTypes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ClientSignersAllowedDocumentType> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) 
                throw new ArgumentNullException(nameof(listQuery));
            
            var companyLegalFormId = query.Val<int?>("CompanyLegalFormId");

            var pre = "csadt.DeleteDate IS NULL";
            pre += companyLegalFormId.HasValue ? " AND csadt.CompanyLegalFormId = @companyLegalFormId" : string.Empty;
            var condition = listQuery.Like(pre, "");
            
            return UnitOfWork.Session.Query<ClientSignersAllowedDocumentType, ClientDocumentType, ClientSignersAllowedDocumentType>($@"
                SELECT * 
                    FROM ClientSignersAllowedDocumentTypes csadt 
                    JOIN ClientDocumentTypes cdt ON csadt.DocumentTypeId = cdt.Id
                    {condition}",
                (c,dt) => { 
                             c.DocumentType = dt;
                             return c;
                          },
                new {
                  companyLegalFormId
                },
                UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var companyLegalFormId = query.Val<int?>("CompanyLegalFormId");

            var pre = "csadt.DeleteDate IS NULL";
            pre += companyLegalFormId.HasValue ? " AND csadt.CompanyLegalFormId = @companyLegalFormId" : string.Empty;
            var condition = listQuery.Like(pre, "");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) 
                    FROM ClientSignersAllowedDocumentTypes csadt 
                    JOIN ClientDocumentTypes cdt ON csadt.DocumentTypeId = cdt.Id
                    {condition}",
                    new
                    {
                      companyLegalFormId
                    },
                    UnitOfWork.Transaction);
        }
    }
}