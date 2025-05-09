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
    public class ClientLegalFormValidationFieldRepository : RepositoryBase, IRepository<ClientLegalFormValidationField>
    {
        public ClientLegalFormValidationFieldRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientLegalFormValidationField entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ClientLegalFormValidationFields(LegalFormId, FieldCode, CreateDate, AuthorId)
                    VALUES(@LegalFormId, @FieldCode, @CreateDate, @AuthorId)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientLegalFormValidationField entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientLegalFormValidationFields SET LegalFormId = @LegalFormId, FieldCode = @FieldCode, CreateDate = @CreateDate, AuthorId = @AuthorId
                    WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientLegalFormValidationFields SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ClientLegalFormValidationField Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientLegalFormValidationField>(@"
                SELECT * 
                FROM ClientLegalFormValidationFields
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ClientLegalFormValidationField Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return UnitOfWork.Session.Query<ClientLegalFormValidationField>(@"
                    SELECT * FROM ClientLegalFormValidationFields", UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ClientLegalFormValidationField> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<ClientLegalFormValidationField>(@"SELECT * FROM ClientLegalFormValidationFields", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ClientLegalFormValidationFields", UnitOfWork.Transaction);
        }

        public List<ClientLegalFormValidationField> ListByLegalForm(int legalFormId)
        {
            return UnitOfWork.Session.Query<ClientLegalFormValidationField>(@"SELECT * FROM ClientLegalFormValidationFields WHERE LegalFormId = @legalFormId", new { legalFormId }, UnitOfWork.Transaction).ToList();
        }
    }
}