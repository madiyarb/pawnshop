using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Verification;
using System;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    [Route("api/verification")]
    public class VerificationController : Controller
    {
        private readonly IVerificationService _verificationService;
        public VerificationController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        [HttpPost("get"), ProducesResponseType(typeof(GetVerificationResponse), 200)]
        public IActionResult Get([FromBody] GetVerificationRequest request)
        {
            ModelState.Validate();
            (int verificationId, DateTime expireDate) = _verificationService.Get(request.ClientId.Value, request.PhoneNumber);
            return Ok(new GetVerificationResponse
            {
                ExpireDate = expireDate,
                VerificationId = verificationId
            });
        }

        [HttpPost("verify")]
        public IActionResult Verify([FromBody] VerifyRequest request)
        {
            ModelState.Validate();
            _verificationService.Verify(request.OTP, request.ClientId.Value);
            return Ok();
        }

        [HttpPost("status"), ProducesResponseType(typeof(GetVerificationStatusResponse), 200)]
        public IActionResult GetVerificationStatus([FromBody]GetVerificationStatusRequest request)
        {
            ModelState.Validate();
            bool doNeedVerification = _verificationService.DoNeedVerification(request.ClientId.Value, request.ContractId);
            return Ok(new GetVerificationStatusResponse { DoNeedVerification = doNeedVerification });
        }
    }
}
