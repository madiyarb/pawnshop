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
    public class ClientSignerRepository : RepositoryBase, IRepository<ClientSigner>
    {
        private readonly ClientRepository _clientRepository;

        public ClientSignerRepository(IUnitOfWork unitOfWork, ClientRepository clientRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
        }

        public void Insert(ClientSigner entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ClientSigners(CompanyId, SignerId, SignerPositionId, DocumentId, CreateDate, AuthorId)
                        VALUES(@CompanyId, @SignerId, @SignerPositionId, @DocumentId, @CreateDate, @AuthorId)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientSigner entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientSigners SET CompanyId = @CompanyId, SignerId = @SignerId, SignerPositionId = @SignerPositionId, 
                        DocumentId = @DocumentId, CreateDate = @CreateDate, AuthorId = @AuthorId
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientSigner Get(int id)
        {
            return UnitOfWork.Session.Query<ClientSigner, Client, ClientDocument, ClientDocumentType, ClientSigner>(@"      
                        SELECT cs.*, cls.*, cd.*, cdt.* 
                        FROM ClientSigners cs
                        JOIN Clients cls ON cls.Id = cs.SignerId
                        JOIN ClientDocuments cd ON cd.Id = cs.DocumentId 
                        JOIN ClientDocumentTypes cdt ON cdt.Id = cd.TypeId
                        WHERE cs.Id = @id
                        AND cs.DeleteDate IS NULL 
                        AND cls.DeleteDate IS NULL 
                        AND cd.DeleteDate IS NULL",
                (cs, cls, cd, cdt) =>
                {
                    cs.Signer = cls;
                    cs.SignerDocument = cd;
                    cs.SignerDocument.DocumentType = cdt;
                    return cs;
                }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ClientSigner Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var companyId = query?.Val<int?>("CompanyId");
            var signerId = query?.Val<int?>("SignerId");

            var condition = @"WHERE cs.DeleteDate IS NULL AND cls.DeleteDate IS NULL AND cd.DeleteDate IS NULL";

            condition += companyId.HasValue ? " AND cs.CompanyId = @companyId" : string.Empty;
            condition += signerId.HasValue ? " AND cs.SignerId = @signerId" : string.Empty;

            return UnitOfWork.Session.Query<ClientSigner, Client, ClientDocument, ClientDocumentType, ClientSigner>($@"
                    SELECT cs.*, cls.*, cd.*, cdt.* 
                        FROM ClientSigners cs
                        JOIN Clients cls ON cls.Id = cs.SignerId
                        JOIN ClientDocuments cd ON cd.Id = cs.DocumentId 
                        JOIN ClientDocumentTypes cdt ON cdt.Id = cd.TypeId
                    {condition}",
                    (cs, cls, cd, cdt) =>
                    {
                        cs.Signer = cls;
                        cs.SignerDocument = cd;
                        cs.SignerDocument.DocumentType = cdt;
                        return cs;
                    },
                new
                {
                    companyId
                }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientSigners SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ClientSigner> List(ListQuery listQuery, object query = null)
        {
            var companyId = query?.Val<int?>("CompanyId");

            var clientSignerList = UnitOfWork.Session.Query<ClientSigner, ClientDocument, ClientDocumentType, ClientSigner>($@"
                SELECT cs.*, cd.*, cdt.* 
                    FROM ClientSigners cs
                    JOIN ClientDocuments cd ON cd.Id = cs.DocumentId 
                        JOIN ClientDocumentTypes cdt ON cdt.Id = cd.TypeId
                        WHERE cs.DeleteDate IS NULL 
                        AND cs.CompanyId = @companyId",
                (cs, cd, cdt) =>
                {
                    cs.SignerDocument = cd;
                    cs.SignerDocument.DocumentType = cdt;
                    return cs;
                }, new { companyId },
                UnitOfWork.Transaction).ToList();

            foreach (var clientSigner in clientSignerList)
                clientSigner.Signer = _clientRepository.Get(clientSigner.SignerId);

            return clientSignerList;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var companyId = query?.Val<int?>("CompanyId");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM ClientSigners WHERE DeleteDate IS NULL AND CompanyId = @companyId", new { companyId },
                    UnitOfWork.Transaction);
        }
    }
}