using System;
using System.Net;

namespace Pawnshop.Web.Models.Contract
{
    public class LoanContractFileResponse : BaseResponseDRPP
    {
        public Guid? FileOtref { get; set; }

        public LoanContractFileResponse(Guid? fileOtref) : base()
        {
            FileOtref = fileOtref;
        }

        public LoanContractFileResponse(HttpStatusCode statusCode, string message, DRPPResponseStatusCode code, Guid? fileOtref = null) : base(statusCode, message, code)
        {
            FileOtref = fileOtref;
        }
    }
}
