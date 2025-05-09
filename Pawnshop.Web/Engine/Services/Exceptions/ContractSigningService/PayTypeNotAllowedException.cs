using System;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public sealed class PayTypeNotAllowedException : Exception
    {
        public PayTypeNotAllowedException(int clientId, int? requisiteId)
            : base($"Для клиента {clientId} у реквизитов с идентификатором {requisiteId} способ вывода не согласуется с источником транша (ТАС ОНЛАЙН)," +
                   $" вывод только на р/с или карту") { }
    }
}
