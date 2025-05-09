using System.Net;

namespace Pawnshop.Services.Contracts.ContractActionOnlineExecutionCheckerService
{
    public sealed class ContractActionOnlineExecutionCheckResult
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Message { get; set; }

        public ContractActionOnlineExecutionErrorType ErrorType { get; set; }

        public bool Failed { get; set; }

        public bool TechnicalIssues { get; set; }


        public ContractActionOnlineExecutionCheckResult(HttpStatusCode statusCode, string message, ContractActionOnlineExecutionErrorType errorType, bool failed, bool technicalIssues = false)
        {
            StatusCode = statusCode;
            Message = message;
            ErrorType = errorType;
            Failed = failed;
            TechnicalIssues = technicalIssues;
        }

        public ContractActionOnlineExecutionCheckResult(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Message = string.Empty;
            Failed = false;
        }
    }
}
