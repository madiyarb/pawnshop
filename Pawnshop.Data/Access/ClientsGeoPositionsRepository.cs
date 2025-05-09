using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ClientsGeoPositions;
using Pawnshop.Data.Models.ClientsGeoPositions.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ClientsGeoPositionsRepository : RepositoryBase
    {
        public ClientsGeoPositionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public ClientGeoPosition Get(Guid id)
        {
            return UnitOfWork.Session.Query<ClientGeoPosition>(@"Select * from ClientsGeopositions where id = @id",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ClientGeoPosition GetLastClientGeoPosition(int clientId)
        {
            return UnitOfWork.Session.Query<ClientGeoPosition>(@"Select top 1 * from ClientsGeopositions where clientId = @clientId Order by CreateDate desc",
                new { clientId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task Insert(ClientGeoPosition clientGeoPosition)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(clientGeoPosition, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientGeoPositionListView GetClientGeoPositions(int clientId, int offset, int limit)
        {
            string query =
                @"Select * from ClientsGeopositions where clientId = @clientId";
            string tail = @$" Order by CreateDate desc OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY";

            string result = query + tail;

            string count = @"SELECT COUNT (*) FROM ClientsGeopositions WHERE clientid = @clientId";

            ClientGeoPositionListView clientGeoPositions = new ClientGeoPositionListView();

            clientGeoPositions.Count = UnitOfWork.Session.Query<int>(count,
                new { clientId }, UnitOfWork.Transaction).FirstOrDefault();

            if (clientGeoPositions.Count == 0)
            {
                return null;
            }

            clientGeoPositions.ClientGeoPositions = UnitOfWork.Session.Query<ClientGeoPosition>(result,
                new { clientId }, UnitOfWork.Transaction).ToList();

            return clientGeoPositions;
        }
    }
}
