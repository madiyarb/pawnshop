using KafkaFlow.Producers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline.Events;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation;
using Pawnshop.Data.Models.Comments;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.Insurance;
using Pawnshop.Web.Models.Estimates;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Reflection.Metadata;
using Pawnshop.Services.ApplicationsOnline.ApplicationOnlineCreditLimitVerification;
using Serilog;
using Pawnshop.Services.CreditLines;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/estimates")]
    [ApiController]
    [Authorize]
    //[Authorize(Permissions.MobileAppAccess)]
    public class EstimatesController : ControllerBase
    {
        private readonly ApplicationOnlinePositionRepository _applicationOnlinePositionRepository;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly IApplicationOnlineService _applicationOnlineService;
        private readonly ApplicationsOnlineEstimationRepository _applicationsOnlineEstimationRepository;
        private readonly CommentsRepository _commentsRepository;
        private readonly IProducerAccessor _producers;
        private readonly ISessionContext _sessionContext;
        private readonly ApplicationOnlineRejectionReasonsRepository _applicationOnlineRejectionReasonsRepository;
        private readonly IApplicationOnlineCreditLimitVerificationService _applicationsOnlineCreditLimitVerificationService;
        private readonly ILogger _logger;
        private readonly ICreditLineService _creditLineService;

        public EstimatesController(
            ApplicationOnlinePositionRepository applicationOnlinePositionRepository,
            ApplicationOnlineRepository applicationOnlineRepository,
            IApplicationOnlineService applicationOnlineService,
            ApplicationsOnlineEstimationRepository applicationsOnlineEstimationRepository,
            CommentsRepository commentsRepository,
            IProducerAccessor producers,
            ISessionContext sessionContext,
            ApplicationOnlineRejectionReasonsRepository applicationOnlineRejectionReasonsRepository,
            IApplicationOnlineCreditLimitVerificationService applicationOnlineCreditLimitVerificationService,
            ILogger logger,
            ICreditLineService creditLineService)
        {
            _applicationOnlinePositionRepository = applicationOnlinePositionRepository;
            _applicationOnlineRepository = applicationOnlineRepository;
            _applicationOnlineService = applicationOnlineService;
            _applicationsOnlineEstimationRepository = applicationsOnlineEstimationRepository;
            _commentsRepository = commentsRepository;
            _producers = producers;
            _sessionContext = sessionContext;
            _applicationOnlineRejectionReasonsRepository = applicationOnlineRejectionReasonsRepository;
            _applicationsOnlineCreditLimitVerificationService = applicationOnlineCreditLimitVerificationService;
            _logger = logger;
            _creditLineService = creditLineService;
        }

        [HttpPost("evaluation-revision")]
        public async Task<ActionResult<EvaluationRevisionResponse>> EvaluationRevision([FromBody] EvaluationRevisionRequest rq)
        {
            var response = new EvaluationRevisionResponse();

            try
            {
                if (!Guid.TryParse(rq?.ApplicatinId, out Guid appId))
                {
                    response.Message = $"Incorrect applicationId {rq.ApplicatinId}!";
                    return BadRequest(response);
                }

                var application = _applicationOnlineRepository.Get(appId);

                if (application == null)
                {
                    response.Message = $"Application {rq.ApplicatinId} not found!";
                    return BadRequest(response);
                }

                if (application.Status != ApplicationOnlineStatus.OnEstimation.ToString() 
                    && application.Status != ApplicationOnlineStatus.RequiresCorrection.ToString() )
                {
                    response.Message = $"Application {rq.ApplicatinId} not editable!";
                    return BadRequest(response);
                }

                var lastEstimate = _applicationsOnlineEstimationRepository.GetLastByApplicationId(appId);

                if (lastEstimate.Status != ApplicationOnlineEstimationStatus.OnEstimation.ToString())
                {
                    response.Message = $"Estimate application {rq.ApplicatinId} not editable!";
                    return BadRequest(response);
                }

                lastEstimate.ValuerName = rq.AuthorName;
                lastEstimate.Status = ApplicationOnlineEstimationStatus.NeedCorrection.ToString();
                application.Status = ApplicationOnlineStatus.RequiresCorrection.ToString();

                var comment = new Comment
                {
                    AuthorName = rq.AuthorName,
                    CommentText = rq.Note,
                    ApplicationOnlineComment = new ApplicationOnlineComment
                    {
                        ApplicationOnlineId = appId,
                        CommentType = ApplicationOnlineCommentTypes.Estimate,
                    }
                };

                using (var transaction = _commentsRepository.BeginTransaction())
                {
                    _commentsRepository.Insert(comment);
                    _applicationsOnlineEstimationRepository.Update(lastEstimate);
                    await _applicationOnlineRepository.Update(application);

                    transaction.Commit();
                }

                await _applicationsOnlineCreditLimitVerificationService.ValidateSumsAndCreateCheck(application);
                response.Success = true;
                return Ok(response);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                response.Message = exception.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpPost("evaluation-result")]
        public async Task<IActionResult> EvaluationResult([FromBody] EvaluationResultRequest rq,
            [FromServices] IApplicationOnlineCheckCreationService checkCreationService)
        {
            var response = new EvaluationResultResponse();
            try
            {
                if (!Guid.TryParse(rq?.ApplicatinId, out Guid appId))
                {
                    response.Message = $"Incorrect applicationId {rq.ApplicatinId}!";
                    return BadRequest(response);
                }

                var application = _applicationOnlineRepository.Get(appId);

                if (application == null)
                {
                    response.Message = $"Application {rq.ApplicatinId} not found!";
                    return BadRequest(response);
                }

                if (application.Status != ApplicationOnlineStatus.OnEstimation.ToString()
                    && application.Status != ApplicationOnlineStatus.EstimationCompleted.ToString()
                    && application.Status != ApplicationOnlineStatus.RequiresCorrection.ToString())
                {
                    response.Message = $"Application {rq.ApplicatinId} not editable!";
                    return BadRequest(response);
                }

                var lastEstimate = _applicationsOnlineEstimationRepository.GetLastByApplicationId(appId);

                var position = _applicationOnlinePositionRepository.Get(application.ApplicationOnlinePositionId);

                if (position == null)
                {
                    response.Message = $"Inccorect application {appId}, not found position!";
                    return StatusCode((int)HttpStatusCode.InternalServerError, response);
                }

                if (rq.Success)
                {
                    var issuedAmountWithLimit = rq.IssuedAmount + await _creditLineService.GetLimitForInsuranceByPosition(rq.EvaluatedAmount);
                    lastEstimate.Status = ApplicationOnlineEstimationStatus.Approved.ToString();
                    lastEstimate.EvaluatedAmount = rq.EvaluatedAmount;
                    lastEstimate.IssuedAmount = issuedAmountWithLimit;
                    position.EstimatedCost = rq.EvaluatedAmount;
                    position.LoanCost = issuedAmountWithLimit;
                    application.Status = ApplicationOnlineStatus.EstimationCompleted.ToString();
                }
                else
                {
                    lastEstimate.Status = ApplicationOnlineEstimationStatus.Decline.ToString();
                    var userId = Constants.ADMINISTRATOR_IDENTITY;
                    if (_sessionContext.IsInitialized)
                        userId = _sessionContext.UserId;
                    var rejectionReason = await _applicationOnlineRejectionReasonsRepository
                        .FindByCode("CarNotPassInspection");
                    if (rejectionReason != null)
                    {
                        application.Reject(userId,rejectionReason.Id, rejectionReason.Code, $"{rejectionReason.InternalReason}." +
                            $" Комментарий оценщика : {rq.Note}");
                    }
                }

                lastEstimate.ValuerName = rq.AuthorName;

                using (var transaction = _commentsRepository.BeginTransaction())
                {
                    var comment = new Comment
                    {
                        AuthorName = rq.AuthorName,
                        CommentText = rq.Note,
                        ApplicationOnlineComment = new ApplicationOnlineComment
                        {
                            ApplicationOnlineId = appId,
                            CommentType = ApplicationOnlineCommentTypes.Estimate,
                        }
                    };

                    _commentsRepository.Insert(comment);
                    _applicationsOnlineEstimationRepository.Update(lastEstimate);
                    _applicationOnlinePositionRepository.Update(position);
                    await _applicationOnlineRepository.Update(application);

                    transaction.Commit();
                }

                if (rq.Success)
                {
                    var oldValue = application.ApplicationAmount;
                    await _applicationOnlineService.ChangeDetailForInsurance(application,
                        Constants.ADMINISTRATOR_IDENTITY, null, null, rq.IssuedAmount);
                    var newValue = application.ApplicationAmount;
                    if (Math.Abs(oldValue - newValue) > (decimal)0.01)
                    {
                        checkCreationService.CreateCheckAttentionApplicationAmountChanged(application.Id, oldValue, newValue);
                    }
                }

                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };
                await _applicationsOnlineCreditLimitVerificationService.ValidateSumsAndCreateCheck(application);

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);

                response.Success = true;
                return Ok(response);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                response.Message = exception.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
    }
}
