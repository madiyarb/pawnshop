using System;

namespace Pawnshop.Services.ApplicationOnlineRefinances
{
    public sealed class ContractsContainIncompleteActionsException : Exception
    {
        public string Message { get; set; }
        public ContractsContainIncompleteActionsException(string message) : base(message)
        {
            Message = message;

        }
    }
}
