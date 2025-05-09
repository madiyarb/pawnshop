using System;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public class ContractClassWrongException : Exception
    {
        public ContractClassWrongException(int contractId, ContractClass contractClass) : base($"Переданный идентификатор договора : {contractId}." +
            $" Не является траншем вызовете другой метод. Класс договора : {contractClass}") { }
    }
}
