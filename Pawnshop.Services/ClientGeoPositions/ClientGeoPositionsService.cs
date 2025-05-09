using System;
using Pawnshop.Data.Access;

namespace Pawnshop.Services.ClientGeoPositions
{
    public sealed class ClientGeoPositionsService : IClientGeoPositionsService
    {
        private readonly ClientsGeoPositionsRepository _clientsGeoPositionsRepository;
        public ClientGeoPositionsService(ClientsGeoPositionsRepository clientsGeoPositionsRepository)
        {
            _clientsGeoPositionsRepository = clientsGeoPositionsRepository ??
                                             throw new ArgumentNullException(nameof(clientsGeoPositionsRepository));
        }
        public bool HasActualGeoPosition(int clientId)
        {
            var geoPosition = _clientsGeoPositionsRepository.GetLastClientGeoPosition(clientId);
            if (geoPosition == null)
            {
                return false;
            }
            if (geoPosition.Date.HasValue)
            {
                if ((DateTime.Now - geoPosition.Date).Value.Hours > 24)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
