using System;
using System.Collections.Generic;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Access
{
    public class ClientFileRowRepository : RepositoryBase, IRepository<ClientFileRow>
    {
        public ClientFileRowRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientFileRow entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
INSERT INTO ClientFileRows
( ClientId, FileRowId )
VALUES ( @ClientId, @FileRowId )", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ClientFileRow entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"DELETE FROM ClientFileRows WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientFileRow Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public ClientFileRow Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var clientId = query?.Val<int?>("ClientId");
            var fileRowId = query?.Val<int?>("FileRowId");

            return UnitOfWork.Session.QuerySingleOrDefault<ClientFileRow>(@"
SELECT TOP 1 *
FROM ClientFileRows
WHERE ClientId = @clientId
    AND FileRowId = @fileRowId", new { clientId, fileRowId });
        }

        public List<ClientFileRow> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }
    }
}