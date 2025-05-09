using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineFileCodes;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Data.Models.ApplicationOnlineFiles.Views;
using Pawnshop.Data.Models.ApplicationOnlineNpck;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Services.ApplicationOnlineFiles;
using Pawnshop.Services.ApplicationOnlineFileStorage;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Exceptions;
using Pawnshop.Services.PDF;
using Pawnshop.Services.TasOnlinePermissionValidator;
using Pawnshop.Web.Extensions;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ApplicationOnlineFiles;
using Serilog;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationOnlineFileController : ControllerBase
    {
        private readonly ISessionContext _sessionContext;
        private readonly ApplicationOnlineFileRepository _applicationOnlineFileRepository;
        private readonly ILogger _logger;
        public ApplicationOnlineFileController(ISessionContext sessionContext, ILogger logger, ApplicationOnlineFileRepository applicationOnlineFileRepository)
        {
            _logger = logger;
            _sessionContext = sessionContext;
            _applicationOnlineFileRepository = applicationOnlineFileRepository;

        }

        [HttpGet("applicationonline/files/{fileid}")]
        [ProducesResponseType(typeof(File), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFile(
            [FromRoute] Guid fileid,
            [FromServices] FileStorageService service,
            CancellationToken cancellationToken)
        {
            var file = await _applicationOnlineFileRepository.Get(fileid);

            if (file == null)
                return NotFound();
            try
            {
                var fileStorageFile = await service.Download(file.StorageFileId, cancellationToken);

                if (fileStorageFile.ContentType == null)
                    return File(fileStorageFile.Stream, file.ContentType);

                return File(fileStorageFile.Stream, fileStorageFile.ContentType);
            }
            catch (ServiceUnavailableException exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, exception.Message);
            }
            catch (Services.ApplicationOnlineFileStorage.Exceptions.UnexpectedResponseException exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost("applicationonline/{applicationonlineid}/file/{code}")]
        public async Task<IActionResult> Upload(
            CancellationToken cancellationToken,
            IFormFile? file,
            [FromRoute] Guid applicationonlineid,
            [FromRoute] string code,
            [FromQuery] bool isAdditional,
            [FromQuery] bool sendToEstimate,
            [FromServices] FileStorageService service,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            [FromServices] ITasOnlinePermissionValidatorService _permissionValidator)
        {
            if (file == null)
            {
                return BadRequest();
            }

            var applicationOnline = applicationOnlineRepository.Get(applicationonlineid);

            if (applicationOnline == null)
            {
                return BadRequest();
            }

            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(applicationOnline))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            if (!applicationOnline.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {applicationOnline.ResponsibleManagerId} назначте заявку себя");
            }
            if (applicationOnline.ListId == null)
            {
                _logger.Error($"У заявки {applicationOnline.Id}, не задан ListId невозможно загрузить файл");
                throw new ArgumentNullException(nameof(ApplicationOnline.ListId));
            }

            var applicationOnlineFileCode =
                applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByCode(code);

            if (applicationOnlineFileCode == null)
                return BadRequest();
            try
            {
                var response = await service.Upload(applicationOnline.ListId.ToString(), file.OpenReadStream(), String.Empty,
                    applicationOnlineFileCode.BusinessType, file.Name, cancellationToken);
                Guid fileid = Guid.NewGuid();
                var fileInfo = new ApplicationOnlineFile(fileid, applicationOnline.Id, Guid.Parse(response.FileGuid),
                    "", file.ContentType, "", () => this.UrlToAction<ApplicationOnlineFileController>(nameof(GetFile),
                        new { fileid }), applicationOnlineFileCode.Id, isAdditional, sendToEstimate);
                await _applicationOnlineFileRepository.Insert(fileInfo);
                return Ok(fileInfo);
            }
            catch (ServiceUnavailableException exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, exception.Message);
            }
            catch (Services.ApplicationOnlineFileStorage.Exceptions.UnexpectedResponseException exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpDelete("applicationonline/files/{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id,
            [FromServices] ApplicationOnlineFileRepository repository)
        {
            var file = await repository.Get(id);
            if (file == null)
                return NotFound();
            file.Delete();
            await repository.Update(file);
            return Ok();
        }

        [HttpGet("applicationOnline/{applicationonlineid}/filelist")]
        public async Task<IActionResult> GetFileList(
            CancellationToken cancellationToken,
            [FromRoute] Guid applicationonlineid,
            [FromQuery] PageBinding pageBinding)
        {
            return Ok(await _applicationOnlineFileRepository.GetListView(applicationonlineid, pageBinding.Offset, pageBinding.Limit));
        }

        [HttpGet("applicationonline/{applicationonlineid}/file/loan-contract/from-mobile")]
        public async Task<ActionResult<ApplicationOnlineFileViewFromMobile>> GetFileListFromMobile(
            CancellationToken cancellationToken,
            [FromRoute] Guid applicationonlineid,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            [FromServices] IContractService contractService,
            [FromServices] IFileStorageService fileStorageService,
            [FromServices] IPdfService pdfService,
            [FromServices] IApplicationOnlineFilesService applicationOnlineFileService)
        {
            try
            {
                var applicationOnline = applicationOnlineRepository.Get(applicationonlineid);

                if (applicationOnline == null)
                    return NotFound(new BaseResponseDRPP(HttpStatusCode.NotFound, $"ApplicationOnline with id : {applicationonlineid} not found",
                        DRPPResponseStatusCode.ApplicationNotFound));

                var loanContractFile = _applicationOnlineFileRepository.GetLoanContractFileFromMobile(applicationonlineid);

                var response = new ApplicationOnlineFileViewFromMobile
                {
                    ContractId = applicationOnline.ContractId ?? 0,
                    Documents = new List<ApplicationOnlineFileFromMobile> { loanContractFile }
                };

                if (!applicationOnline.ContractId.HasValue)
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.NotFound,
                        $"По данной заявке еще не создан договор", DRPPResponseStatusCode.BadData));

                if (loanContractFile != null &&
                    loanContractFile.CreateDate.Date == DateTime.Now.Date)
                {
                    return Ok(response);
                }

                var contract = contractService.GetOnlyContract(applicationOnline.ContractId.Value);

                if ((int)contract.Status >= (int)ContractStatus.Signed)
                    return Ok(response);

                Guid fileid = Guid.NewGuid();
                var createLoanContractFileResult = applicationOnlineFileService.CreateLoanContractFile(applicationOnline, out string error,
                    () => this.UrlToAction<ApplicationOnlineFileController>(nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                    fileid, null, cancellationToken);

                if (!createLoanContractFileResult)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new BaseResponseDRPP(HttpStatusCode.InternalServerError, error,
                            DRPPResponseStatusCode.FileCreationFailed));
                }

                loanContractFile = _applicationOnlineFileRepository.GetLoanContractFileFromMobile(applicationonlineid);

                return Ok(new ApplicationOnlineFileViewFromMobile
                {
                    ContractId = applicationOnline.ContractId ?? 0,
                    Documents = new List<ApplicationOnlineFileFromMobile> { loanContractFile }
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

        [HttpPost("applicationonline/{applicationonlineid}/create")]
        public async Task<ActionResult> CreateFileFromMobile(
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            [FromRoute] Guid applicationonlineid,
            [FromBody] ApplicationOnlineFileFromMobileCreationBinding binding,
            CancellationToken cancellationToken)
        {
            var fileCode = applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByBusinessType(binding.BusinessType);

            if (fileCode == null)
            {
                fileCode =
                    new ApplicationOnlineFileCode(Guid.NewGuid(),
                        binding.BusinessType,
                        binding.BusinessType, binding.BusinessType, binding.BusinessType, binding.BusinessType);
                applicationOnlineFileCodesRepository.Insert(fileCode);
            }

            var contentType = "image/png";

            if (!string.IsNullOrEmpty(binding.ContentType))
            {
                contentType = binding.ContentType;
            }

            Guid fileId = Guid.NewGuid();
            var fileInfo = new ApplicationOnlineFile(Guid.NewGuid(), applicationonlineid, binding.FileId,
                contentType, contentType, "", () => this.UrlToAction<ApplicationOnlineFileController>(nameof(GetFile),
                    new { fileId }), fileCode.Id, false, false);
            await _applicationOnlineFileRepository.Insert(fileInfo);
            return NoContent();
        }

        [HttpPost("{applicationid}/file/{filestorageid}")]
        [AllowAnonymous]
        public async Task<IActionResult> SaveNewFileInfo(
            [FromRoute] Guid applicationid,
            [FromRoute] Guid filestorageid,
            [FromBody] ApplicationOnlineFileNewInfoBinding binding,
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            [FromServices] ApplicationOnlineNpckSignFileRepository applicationOnlineNpckSignFileRepository)
        {
            try
            {
                var applicationOnlineFileCode = applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByBusinessType(binding.BusinessType);

                Guid fileid = Guid.NewGuid();
                var fileInfo = new ApplicationOnlineFile(fileid, applicationid, filestorageid, "", binding.ContentType, "",
                    () => this.UrlToAction<ApplicationOnlineFileController>(nameof(GetFile), new { fileid }),
                    applicationOnlineFileCode.Id);

                await _applicationOnlineFileRepository.Insert(fileInfo);

                if (binding.EsignFileId.HasValue)
                    applicationOnlineNpckSignFileRepository.Insert(new ApplicationOnlineNpckSignFile(fileid, binding.EsignFileId.Value));

                return Ok(fileInfo);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
