using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineApprovedOtherPayment;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Services.ApplicationOnlineFileStorage;
using Pawnshop.Web.Extensions;
using Pawnshop.Web.Models.ApplicationOnlineApprovedOtherPayment;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Serilog;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/applicationOnline/approvedOtherPaymnets")]
    [ApiController]
    [Authorize]
    public class ApplicationOnlineApprovedOtherPaymentsController : ControllerBase
    {
        private readonly ApplicationOnlineFileCodesRepository _applicationOnlineFileCodesRepository;
        private readonly ApplicationOnlineFileRepository _applicationOnlineFileRepository;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly ApplicationOnlineApprovedOtherPaymentRepository _approvedOtherPaymentRepository;
        private readonly FileStorageService _fileStorageService;
        private readonly ISessionContext _sessionContext;
        private readonly ILogger _logger;

        public ApplicationOnlineApprovedOtherPaymentsController(
            ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            ApplicationOnlineFileRepository applicationOnlineFileRepository,
            ApplicationOnlineRepository applicationOnlineRepository,
            ApplicationOnlineApprovedOtherPaymentRepository approvedOtherPaymentRepository,
            FileStorageService fileStorageService,
            ISessionContext sessionContext,
            ILogger logger)
        {
            _applicationOnlineFileCodesRepository = applicationOnlineFileCodesRepository;
            _applicationOnlineFileRepository = applicationOnlineFileRepository;
            _applicationOnlineRepository = applicationOnlineRepository;
            _approvedOtherPaymentRepository = approvedOtherPaymentRepository;
            _fileStorageService = fileStorageService;
            _sessionContext = sessionContext;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApprovedOtherPaymentView>> CreateApprovedOtherPayment([FromForm] ApprovedOtherPaymentCreateView view, CancellationToken cancellationToken)
        {
            if (view.File == null || view.File.Length == 0)
                return BadRequest("Ошибка чтения файла.");

            var application = _applicationOnlineRepository.Get(view.ApplicationOnlineId);

            if (application == null)
                return BadRequest($"Заявка {view.ApplicationOnlineId} не найдена.");

            if (!application.CanEditing(_sessionContext.UserId))
                return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");

            var entity = new ApplicationOnlineApprovedOtherPayment
            {
                Amount = view.Amount,
                ApplicationOnlineId = view.ApplicationOnlineId,
                CreateBy = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                SubjectName = view.SubjectName,
            };

            var applicationOnlineFileCode = _applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByBusinessType(Constants.APPLICATION_ONLINE_FILE_BUSINESS_TYPE_OTHER);

            try
            {
                var response = await _fileStorageService.Upload(application.ListId.ToString(), view.File.OpenReadStream(), String.Empty,
                    applicationOnlineFileCode.BusinessType, view.File.Name, cancellationToken);

                Guid fileid = Guid.NewGuid();

                var fileInfo = new ApplicationOnlineFile(fileid, application.Id, Guid.Parse(response.FileGuid),
                    "", view.File.ContentType, "", () => this.UrlToAction<ApplicationOnlineFileController>(nameof(ApplicationOnlineFileController.GetFile),
                        new { fileid }), applicationOnlineFileCode.Id);

                await _applicationOnlineFileRepository.Insert(fileInfo);

                entity.FileId = fileInfo.Id;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ошибка загрузки документа:\r\n{exception.Message}");
            }

            _approvedOtherPaymentRepository.Insert(entity);

            var createdEntity = _approvedOtherPaymentRepository.Get(entity.Id);

            return Ok(ToView(createdEntity));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var entity = _approvedOtherPaymentRepository.Get(id);

            if (entity == null)
                return NotFound();

            var application = _applicationOnlineRepository.Get(entity.ApplicationOnlineId);

            if (!application.CanEditing(_sessionContext.UserId))
                return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");

            _approvedOtherPaymentRepository.Delete(id);

            return Ok();
        }

        [HttpGet("list")]
        public ActionResult<ApprovedOtherPaymentListView> GetListByApplicationOnlineId([FromQuery] Guid applicationOnlineId)
        {
            var application = _applicationOnlineRepository.Get(applicationOnlineId);

            if (application == null)
                return NotFound($"Заявка {applicationOnlineId} не найдена!");

            var entities = _approvedOtherPaymentRepository.List(null, new { ApplicationOnlineId = applicationOnlineId });

            var response = new ApprovedOtherPaymentListView
            {
                List = entities.Select(x => ToView(x)).ToList()
            };

            return Ok(response);
        }


        private ApprovedOtherPaymentView ToView(ApplicationOnlineApprovedOtherPayment entity)
        {
            if (entity == null)
                return null;

            return new ApprovedOtherPaymentView
            {
                Amount = entity.Amount,
                CreateBy = entity.CreateBy,
                CreateByName = entity.Author?.Fullname,
                CreateDate = entity.CreateDate,
                FileId = entity.File?.Id,
                FileUrl = entity?.File.Url,
                Id = entity.Id,
                SubjectName= entity.SubjectName,
            };
        }
    }
}
