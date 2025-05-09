namespace Pawnshop.Services.ClientGeoPositions
{
    public interface IClientGeoPositionsService
    {
        public bool HasActualGeoPosition(int clientId);
    }
}
