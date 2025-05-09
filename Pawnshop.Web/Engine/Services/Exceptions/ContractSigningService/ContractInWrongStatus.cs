using System;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public class ContractInWrongStatus : Exception
    {
        public ContractInWrongStatus(int contractId, ContractStatus status) : base($"Переданный идентификатор договора : {contractId}." +
            $"Не находиться в статусе черновика: {status} невозможно подписать")
        { }
    }
}
