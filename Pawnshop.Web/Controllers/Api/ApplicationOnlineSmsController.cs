using KafkaFlow.Producers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineSms.Bindings;
using Pawnshop.Data.Models.ApplicationOnlineSms.Views;
using Pawnshop.Data.Models.ApplicationsOnline.Events;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.ApplicationOnlineSms;
using Pawnshop.Web.Models;
using Serilog;
using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class ApplicationOnlineSmsController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly IProducerAccessor _producers;
        private readonly ILogger _logger;
        private readonly EnviromentAccessOptions _options;

        public ApplicationOnlineSmsController(ISessionContext context, IProducerAccessor producers, ILogger logger, IOptions<EnviromentAccessOptions> options)
        {
            _sessionContext = context;
            _producers = producers;
            _logger = logger;
            _options = options.Value;
        }

        [HttpPost("applicationonline/{id}/sign/sms/send")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> SendSmsForContractSign(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] IApplicationOnlineSmsService applicationOnlineSmsService,
            [FromQuery] string phoneNumber)
        {
            var application = repository.Get(id);
            if (application == null)
            {
                return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Application : {id} not found",
                    DRPPResponseStatusCode.ApplicationNotFound));
            }

            if (application.SignType != ApplicationOnlineSignType.SMS)
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, "Method not active!",
                        DRPPResponseStatusCode.UnspecifiedProblem));

            try
            {
                var verification = applicationOnlineSmsService.SendSmsForSign(application.Id, phoneNumber);
                return Ok(new ApplicationOnlineSmsSendView
                {
                    PhoneNumber = verification.PhoneNumber
                });
            }
            catch (PawnshopApplicationException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.Locked,
                    new BaseResponseDRPP(HttpStatusCode.Locked, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedPawnshopApplicationException));
            }
            catch (DbException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InsufficientStorage,
                    new BaseResponseDRPP(HttpStatusCode.InsufficientStorage, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedDatabaseProblems));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedProblem));
            }

        }

        [HttpPost("applicationonline/{id}/sign/sms/validate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> ValidateSms(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineSignOtpVerificationRepository applicationOnlineSignOtpVerificationRepository,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ContractAdditionalInfoRepository contractAdditionalInfoRepository,
            [FromBody] ApplicationOnlineValidationSmsBinding binding)
        {
            try
            {
                var application = applicationOnlineRepository.Get(id);
                if (application == null)
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Application : {id} not found",
                        DRPPResponseStatusCode.ApplicationNotFound));
                }

                if (application.SignType != ApplicationOnlineSignType.SMS)
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new BaseResponseDRPP(HttpStatusCode.InternalServerError, "Method not active!",
                            DRPPResponseStatusCode.UnspecifiedProblem));

                var verification = applicationOnlineSignOtpVerificationRepository.Get(id);
                if (verification == null)
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Verification request for application :  {id} not found",
                        DRPPResponseStatusCode.VerificationRequestNotFound));

                if (verification.Success.HasValue)
                {
                    if (verification.Success.Value)
                    {
                        return Ok(new ApplicationOnlineValidationSmsView
                        {
                            AttemptsLeft = verification.RetryCount - verification.TryCount,
                            RetryCount = verification.TryCount,
                            Success = verification.Success.Value
                        });
                    }
                }

                if (verification.TryCount == verification.RetryCount)
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"No more attempts left",
                        DRPPResponseStatusCode.NoMoreAttemptsLeft));
                }

                verification.Verify(binding.SmsCode);

                applicationOnlineSignOtpVerificationRepository.Update(verification);

                if (verification.Success.HasValue && verification.Success.Value)
                {
                    application.ChangeStatus(ApplicationOnlineStatus.RequisiteCheck, _sessionContext.UserId);
                    await applicationOnlineRepository.Update(application);
                    var message = new ApplicationOnlineStatusChanged
                    {
                        ApplicationOnline = application,
                        Status = application.Status
                    };

                    if (application.ContractId.HasValue)
                    {
                        var contractAddInfo = contractAdditionalInfoRepository.Get(application.ContractId.Value);

                        if (contractAddInfo == null)
                        {
                            contractAddInfo = new ContractAdditionalInfo
                            {
                                Id = application.ContractId.Value,
                                SmsCode = verification.Code
                            };

                            contractAdditionalInfoRepository.Insert(contractAddInfo);
                        }
                        else
                        {
                            contractAddInfo.SmsCode = verification.Code;
                            contractAdditionalInfoRepository.Update(contractAddInfo);
                        }
                    }

                    await _producers["ApplicationOnline"]
                        .ProduceAsync(application.Id.ToString(), message);

                }
                return Ok(new ApplicationOnlineValidationSmsView
                {
                    AttemptsLeft = verification.RetryCount - verification.TryCount,
                    RetryCount = verification.TryCount,
                    Success = verification.Success
                });
            }
            catch (PawnshopApplicationException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.Locked,
                    new BaseResponseDRPP(HttpStatusCode.Locked, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedPawnshopApplicationException));
            }
            catch (DbException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InsufficientStorage,
                    new BaseResponseDRPP(HttpStatusCode.InsufficientStorage, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedDatabaseProblems));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }
    }
}
