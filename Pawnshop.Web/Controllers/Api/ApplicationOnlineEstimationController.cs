using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow.Producers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ApplicationsOnline.Events;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation;
using Pawnshop.Services.Estimation;
using Pawnshop.Services.Estimation.Exceptions;
using Pawnshop.Services.Estimation.Images;
using Pawnshop.Services.Estimation.v2;
using Pawnshop.Services.Exceptions;
using Pawnshop.Web.Models;
using Serilog;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationOnlineEstimationController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly IProducerAccessor _producers;
        private readonly ILogger _logger;
        public ApplicationOnlineEstimationController(ISessionContext sessionContext,
            IProducerAccessor producers,
            ILogger logger)
        {
            _sessionContext = sessionContext;
            _producers = producers;
            _logger = logger;
        }

        [Authorize(Permissions.TasOnlineVerificator)]
        [HttpPost("applications/{id}/sendtoestimation/old")]
        [ProducesResponseType(typeof(ApplicationsOnlineEstimation), 200)]
        [ProducesResponseType(typeof(string), 422)]
        [ProducesResponseType(typeof(string), 503)]
        [ProducesResponseType(typeof(string), 500)]
        [Obsolete]
        public async Task<IActionResult> SendToEstimationOld(
            [FromRoute] Guid id,
            [FromServices] IApplicationOnlineEstimationImageService estimationImageService,
            [FromServices] OldEstimationService estimationService,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            CancellationToken cancellationToken)
        {

            #region Проверка


            var application = applicationOnlineRepository.Get(id);

            if (!application.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");
            }

            #endregion


            #region Загрузка картинок для оценщиков

            try
            {
                await estimationImageService.UploadImagesToEstimationService(id, cancellationToken);
            }
            catch (ServiceUnavailableException exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, exception.Message);
            }
            catch (Services.Estimation.Exceptions.UnexpectedResponseException exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }

            #endregion


            #region Отправка в приложение оценки

            try
            {
                var estimation =
                    await estimationService.SendToEstimation(id, _sessionContext.UserId, cancellationToken);
                application = applicationOnlineRepository.Get(id);
                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };
                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);
                return Ok(estimation);
            }
            catch (NotEnoughDataForEstimationException exception)
            {
                _logger.Warning(exception, exception.Message);
                return UnprocessableEntity(new BaseResponse(HttpStatusCode.UnprocessableEntity,
                    exception.Message));
            }
            catch (ServiceUnavailableException exception) //эти должны в сервисе записаться
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new BaseResponse(HttpStatusCode.ServiceUnavailable, exception.Message));
            }
            catch (Services.Estimation.Exceptions.UnexpectedResponseException exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponse(HttpStatusCode.ServiceUnavailable, exception.Message));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponse(HttpStatusCode.ServiceUnavailable, exception.Message));
            }

            #endregion
        }

        [Authorize(Permissions.TasOnlineVerificator)]
        [HttpPost("applications/{id}/sendtoestimation")]
        [ProducesResponseType(typeof(ApplicationsOnlineEstimation), 200)]
        [ProducesResponseType(typeof(string), 422)]
        [ProducesResponseType(typeof(string), 503)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> SendToEstimation(
            [FromRoute] Guid id,
            [FromServices] IApplicationOnlineEstimationImageService estimationImageService,
            [FromServices] EstimationService estimationService,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            CancellationToken cancellationToken)
        {
            var application = applicationOnlineRepository.Get(id);

            if (!application.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");
            }

            if (!(application.Status == ApplicationOnlineStatus.Verification.ToString()
                || application.Status == ApplicationOnlineStatus.RequiresCorrection.ToString()))
            {
                return BadRequest($"Заявка должна быть в статусе \"{ApplicationOnlineStatus.Verification.GetDisplayName()}\" или \"{ApplicationOnlineStatus.RequiresCorrection.GetDisplayName()}\".");
            }

            try
            {
                var estimation = await estimationService.SendToEstimation(id, cancellationToken);

                application.ChangeStatus(ApplicationOnlineStatus.OnEstimation, _sessionContext.UserId);
                await applicationOnlineRepository.Update(application);

                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);

                return Ok(estimation);
            }
            catch (NotEnoughDataForEstimationException exception)
            {
                _logger.Warning(exception, exception.Message);
                return UnprocessableEntity(new BaseResponse(HttpStatusCode.UnprocessableEntity,
                    exception.Message));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponse(HttpStatusCode.ServiceUnavailable, exception.Message));
            }
        }

        [HttpGet("applicationonlineestimation/{id}/getbyapplicationid")]
        [ProducesResponseType(typeof(ApplicationsOnlineEstimation), 200)]
        public async Task<IActionResult> GetLastEstimation([FromRoute] Guid id,
            [FromServices] ApplicationsOnlineEstimationRepository repository)
        {
            return Ok(repository.GetLastByApplicationId(id));
        }

        [HttpGet("applicationonlineestimation/{id}/list")]
        public async Task<IActionResult> GetFileList(
            CancellationToken cancellationToken,
            [FromRoute] Guid id,
            [FromQuery] PageBinding pageBinding,
            [FromServices] ApplicationsOnlineEstimationRepository repository)
        {
            var list = repository.GetListView(id, pageBinding.Offset, pageBinding.Limit);
            if (list == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, "Оценок не найдено"));
            }
            return Ok(list);
        }
    }
}
