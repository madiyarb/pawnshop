using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Access
{

    /// <summary>
    /// TODO В ЭТОТ REPOSITORY Должны переехать методы по CRUD ClientAddress из ClientRepository при рефакторинге CLIENT REPOSITORY 
    /// </summary>
    public sealed class ClientAddressesRepository : RepositoryBase
    {
        public ClientAddressesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Delete(ClientAddress address)
        {
            UnitOfWork.Session.Execute(@"
            UPDATE ClientAddresses
            SET DeleteDate = @DeleteDate, IsActual = @IsActual
            WHERE Id = @id", address, UnitOfWork.Transaction);
        }
    }
}
