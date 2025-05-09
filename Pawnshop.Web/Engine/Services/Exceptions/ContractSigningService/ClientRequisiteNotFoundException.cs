using System;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public class ClientRequisiteNotFoundException : Exception
    {
        public ClientRequisiteNotFoundException(int clientId, int? requisiteId) 
            : base($"Для клиента {clientId} не найдены реквизиты по умолчанию или переданные {requisiteId}") { }
    }
}
