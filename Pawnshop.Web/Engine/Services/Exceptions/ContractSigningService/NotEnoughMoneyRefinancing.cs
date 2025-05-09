using System;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public sealed class NotEnoughMoneyRefinancing : Exception
    {
        public NotEnoughMoneyRefinancing(string message) : base(message) { }
    }
}
