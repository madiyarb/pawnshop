using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Access
{
    public class ContractDocumentRepository : RepositoryBase, IRepository<ContractDocument>
    {
        public ContractDocumentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public ContractDocument Find(object query)
        {
            if(query == null) throw new ArgumentException(nameof(query));

            var contractId = query?.Val<int?>("ContractId");
            var templateId = query?.Val<int?>("TemplateId");
            if (!contractId.HasValue) throw new ArgumentOutOfRangeException(nameof(contractId));
            if (!templateId.HasValue) throw new ArgumentOutOfRangeException(nameof(templateId));

            return UnitOfWork.Session.Query<ContractDocument>(@$"SELECT TOP 1 * FROM ContractDocuments
WHERE DeleteDate IS NULL AND ContractId = @contractId AND TemplateId = @templateId
ORDER BY Id DESC",
                new { templateId, contractId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ContractDocument Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(ContractDocument entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"INSERT INTO ContractDocuments(ContractId, TemplateId, Number, AuthorId, CreateDate)
                VALUES(@ContractId, @TemplateId, @Number, @AuthorId, @CreateDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractDocument> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(ContractDocument entity)
        {
            throw new NotImplementedException();
        }
    }
}
