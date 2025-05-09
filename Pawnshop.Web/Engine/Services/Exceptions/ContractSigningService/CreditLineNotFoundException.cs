using System;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public class CreditLineNotFoundException : Exception
    {
        public CreditLineNotFoundException(int creditLineId) : base($"Договор с идентификатором : {creditLineId} не найден") { }
    }
}
