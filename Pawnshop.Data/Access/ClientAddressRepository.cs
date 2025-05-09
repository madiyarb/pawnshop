using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries.Address;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Core.Queries;
using System.Threading.Tasks;
using Dapper;

namespace Pawnshop.Data.Access
{
    public class ClientAddressRepository : RepositoryBase, IRepository<ClientAddress>
    {
        public ClientAddressRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientAddress entity)
        {
            throw new System.NotImplementedException();
        }

        public void Update(ClientAddress entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public ClientAddress Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ClientAddress> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ClientAddress>(@"
                SELECT ca.*
                FROM ClientAddresses ca
                WHERE ca.Id=@id AND ca.DeleteDate IS NULL",
            new { id }, UnitOfWork.Transaction);
        }

        public List<ClientAddress> Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<ClientAddress> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        ClientAddress IRepository<ClientAddress>.Find(object query)
        {
            throw new NotImplementedException();
        }
    }
}
