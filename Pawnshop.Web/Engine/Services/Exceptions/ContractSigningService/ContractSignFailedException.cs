using System;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public class ContractSignFailedException : Exception
    {
        public ContractSignFailedException(string message) : base(message) { }
    }
}
