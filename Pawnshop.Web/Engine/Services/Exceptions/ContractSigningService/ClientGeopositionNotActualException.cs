using System;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public class ClientGeopositionNotActualException : Exception
    {
        public ClientGeopositionNotActualException() : base("Геопозиция клиента не отмечалась на протяжении 24 часов") { }
    }
}
