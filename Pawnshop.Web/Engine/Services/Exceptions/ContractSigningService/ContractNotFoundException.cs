using System;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public class ContractNotFoundException : Exception
    {
        public ContractNotFoundException(int contractId) : base($"Договор с идентификатором : {contractId} не найден") { }
    }
}
