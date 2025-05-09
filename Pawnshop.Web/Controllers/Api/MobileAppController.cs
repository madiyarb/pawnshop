using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Data.Models.Parking;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.HardCollection.Command;
using Pawnshop.Services.HardCollection.Command.Interfaces;
using Pawnshop.Services.Parkings.History;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Services.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.MobileAppAccess)]
    public class MobileAppController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly IContractService _contractService;
        private readonly IVerificationService _verificationService;
        private readonly IParkingHistoryService _parkingHistoryService;

        private readonly IMediator _mediator;
        public MobileAppController(
            IApplicationService applicationService,
            IContractService contractService,
            IVerificationService verificationService,
            IParkingHistoryService parkingHistoryService,
            IMediator mediator
        )
        {
            _applicationService = applicationService;
            _contractService = contractService;
            _verificationService = verificationService;
            _parkingHistoryService = parkingHistoryService;
            _mediator = mediator;
        }

        [HttpGet("/api/mobileApp/same-applies")]
        public SameContractList GetSameContracts(MobileAppModel mobileAppModel)
        {
            if (mobileAppModel is null)
                throw new PawnshopApplicationException("Проверьте данные");

            ModelState.Validate();

            return _applicationService.GetSameContracts(mobileAppModel);
        }

        [HttpPost("/api/mobileApp/black-list-share")]
        [Event(EventCode.DictVehiclesBlackListItemSaved, EventMode = EventMode.All)]
        public IActionResult SendCarToBlackList([FromBody] ModelForBlackList model)
        {
            if (model is null)
                throw new PawnshopApplicationException("Проверьте данные");

            ModelState.Validate();

            return Ok(_applicationService.SendCarOrClientToBlackList(model));
        }

        [HttpPost("/api/mobileApp/apply")]
        [Event(EventCode.ApplicationController, EventMode = EventMode.All, EntityType = EntityType.Application)]
        public IActionResult SaveApprovedContract([FromBody] ApprovedContract model)
        {
            if (model is null)
                throw new PawnshopApplicationException("Проверьте данные");

            _applicationService.Validate(model);

            return Ok(_applicationService.Save(model));
        }

        [HttpGet("/api/mobileApp/black-list-check")]
        public IActionResult CheckForBlackList(MobileAppModel mobileAppModel)
        {
            if (mobileAppModel is null)
                throw new PawnshopApplicationException("Проверьте данные");

            return Ok(_applicationService.CheckForBlackList(mobileAppModel));
        }

        [HttpGet("/api/mobileApp/get-marks")]
        public IActionResult GetVehicleMarks() => Ok(_applicationService.GetVehicleMarks());

        [HttpGet("/api/mobileApp/get-models-by-mark-id")]
        public IActionResult GetVehicleModels(int markId) => Ok(_applicationService.GetVehicleModelsByMarkId(markId));

        [HttpGet("/api/mobileApp/addition")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(PawnshopApplicationException))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContractDataForAddition))]
        public IActionResult GetContractDataForAddition([FromQuery] string bodyNumber)
        {
            try
            {
                return Ok(_applicationService.GetContractDataForAddition(bodyNumber));
            }
            catch (PawnshopApplicationException exception)
            {
                return NotFound(exception);
            }
        }

        [HttpGet("/api/mobileApp/addition-new")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(PawnshopApplicationException))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContractDataForAddition))]
        public async Task<IActionResult> GetContractDataForAdditionNew([FromQuery] string bodyNumber, [FromQuery] string identityNumber, [FromQuery] string techPassportNumber)
        {
            try
            {
                return Ok(await _applicationService.GetContractDataForAdditionAsync(bodyNumber, identityNumber, techPassportNumber));
            }
            catch (PawnshopApplicationException exception)
            {
                return NotFound(exception);
            }
        }

        [HttpGet("/api/mobileApp/credit-line-tranche")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(PawnshopApplicationException))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContractDataForTranche))]
        public async Task<IActionResult> GetContractDataForTranche([FromQuery] string bodyNumber, [FromQuery] string identityNumber, [FromQuery] string techPassportNumber)
        {
            try
            {
                return Ok(await _applicationService.GetContractDataForTrancheAsync(bodyNumber, identityNumber, techPassportNumber));
            }
            catch (PawnshopApplicationException exception)
            {
                return NotFound(exception);
            }
        }

        [HttpGet("/api/mobileApp/check-identity")]
        public IActionResult GetIdentityDocumentValidation([FromQuery] string identityNumber, [FromQuery] string documentNumber)
        {
            return Ok(_applicationService.CheckIdentityDocument(identityNumber,documentNumber));
        }

        [HttpPost("/api/mobileApp/reissue-autocredit")]
        public IActionResult ReissueAutocredit([FromBody] ReissueCarModel reissueCarModel)
        {
            if (reissueCarModel is null)
                throw new PawnshopApplicationException("Проверьте данные");

            return Ok(_contractService.ReissueAutocredit(reissueCarModel));
        }

        [HttpPost("/api/mobileApp/hardCollection-addClientAddress")]
        public IActionResult HCAddClientAddress([FromBody] AddClientAddressCommand request)
        {
            if (request is null)
                throw new PawnshopApplicationException("Проверьте данные");

            var addressId = _mediator.Send(request).Result;

            return Ok(addressId);
        }

        [HttpPost("/api/mobileApp/hardCollection-addClientContact")]
        public IActionResult HCAddClientContact([FromBody] AddClientContactCommand request)
        {
            if (request is null)
                throw new PawnshopApplicationException("Проверьте данные");

            var clientContactId = _mediator.Send(request).Result;

            return Ok(clientContactId);
        }

        [HttpPost("/api/mobileApp/hardCollection-addClientAdditionalContact")]
        public IActionResult HCAddClientAdditionalContact([FromBody] AddClientAdditionalContactCommand request)
        {
            if (request is null)
                throw new PawnshopApplicationException("Проверьте данные");

            var additionalContactId = _mediator.Send(request).Result;

            return Ok(additionalContactId);
        }

        [HttpPost("/api/mobileApp/hardCollection-addWitness")]
        public IActionResult HCAddWitness([FromBody] AddWitnessCommand request, [FromServices] BranchContext branchContext)
        {
            if (request is null)
                throw new PawnshopApplicationException("Проверьте данные");

            var clientId = _mediator.Send(request).Result;

            branchContext.Init(530);//TODO need fix with? 
            _verificationService.Get(clientId, request.MobilePhone, false);

            return Ok(clientId);
        }

        [HttpPost("/api/mobileApp/hardCollection-verifyWitness")]
        public IActionResult HCVerifyWitnessSms([FromBody] SmsVerficationWitnessCommand request)
        {
            if (request is null)
                throw new PawnshopApplicationException("Проверьте данные");

            var isSucceeded = _mediator.Send(request).Result;

            return Ok(isSucceeded);
        }

        [HttpPost("/api/mobileApp/hardCollection-verifyAcceptanceCertificate")]
        public IActionResult HCVerifyAcceptanceCertificate([FromBody] SmsVerificationCertCommand request)
        {
            if (request is null)
                throw new PawnshopApplicationException("Проверьте данные");

            var isSucceeded = _mediator.Send(request).Result;

            return Ok(isSucceeded);
        }

        [HttpPost("/api/mobileApp/hardCollection-acceptanceCertificate")]
        public IActionResult HCAcceptanceCertificate([FromBody] SendSmsCertCommand request,
            [FromServices] BranchContext branchContext)
        {
            if (request is null)
                throw new PawnshopApplicationException("Проверьте данные");

            branchContext.Init(530);//TODO need fix this?
            var verify = _verificationService.Get(request.ClientId, request.MobilePhone, false);

            request.VerificationId = verify.Item1;
            var isSucceeded = _mediator.Send(request).Result;

            return Ok(isSucceeded);
        }

        [HttpPost("/api/mobileApp/hardCollection-saveCertificate")]
        public IActionResult HCSaveCertificate([FromForm] UploadFileCommand request,
            [FromServices] EventLog _eventLog,
            [FromServices] FileRepository _fileRepository,
            [FromServices] IStorage _storage)
        {
            var file = request.File;
            if (file == null)
            {
                return NoContent();
            }

            FileRow fileRow;

            using (var transaction = _fileRepository.BeginTransaction())
            {
                fileRow = new FileRow
                {
                    CreateDate = DateTime.Now,
                    ContentType = file.ContentType,
                    FileName = Path.GetFileName(file.FileName),
                    FilePath = _storage.Save(file.OpenReadStream()).Result
                };
                _fileRepository.Insert(fileRow);
                _eventLog.Log(EventCode.FileUploaded, EventStatus.Success, null, null, fileRow.FileName, fileRow.FilePath);

                transaction.Commit();
            }

            request.FileRowId = fileRow.Id;

            _mediator.Send(request).Wait();

            return Ok(fileRow.Id);
        }

        [HttpPost("/api/mobileApp/hardCollection-addComment")]
        public IActionResult HCAddComment([FromBody] AddCommentCommand model)
        {
            var result = _mediator.Send(model).Result;

            return Ok(1);
        }

        [HttpGet("/api/mobileApp/hardCollection-getHistoryList")]
        public IActionResult HCGetHistoryList([FromQuery] int contractId)
        {
            var historyList = _mediator.Send(new GetHistoryListQuery() { ContractId = contractId }).Result;

            return Ok(historyList);
        }

        [HttpPost("/api/mobileApp/hardCollection-sendToParking")]
        public IActionResult HCSendToParking([FromBody] SendToParking request)
        {
            _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId }).Wait();

            var parkingHistory = new ParkingHistory()
            {
                ContractId = request.ContractId,
                Date = DateTime.Now.Date,
                DelayDays = 0,
                ParkingActionId = 1
            };

            _parkingHistoryService.Save(parkingHistory);

            var result = _mediator.Send(new CloseHardCollectionCommand() 
            { 
                ContractId = request.ContractId, 
                AuthorId = request.AuthorId 
            }).Result;

            return Ok(result);
        }

        [HttpPost("/api/mobileApp/hardCollection-notLiveInAddress")]
        public IActionResult HCNotLiveInAddress([FromBody] UpdateNotLiveInAddressCommand request)
        {
            var result = _mediator.Send(request).Result;

            return Ok(result);
        }

        [HttpPost("/api/mobileApp/hardCollection-notMoveToParkingCar")]
        public IActionResult HCNotMoveToParkingCar([FromBody] UpdateNotSeizedCommand request)
        {
            var geoId = _mediator.Send(request).Result;

            return Ok(geoId);
        }

        [HttpPost("/api/mobileApp/hardCollection-sendToLegal")]
        public IActionResult HCSendToLegal([FromBody] UpdateCollectionToLegalHardCommand request)
        {
            var isChanged = _mediator.Send(request).Result;

            return Ok(isChanged);
        }

        [HttpPost("/api/mobileApp/hardCollection-addToMyToDoList")]
        public IActionResult HCAddToMyToDoList([FromBody] AddToDoMyListCommand request)
        {
            var isChanged = _mediator.Send(request).Result;

            return Ok(isChanged);
        }

        [HttpPost("/api/mobileApp/hardCollection-AddExpence")]
        public IActionResult HCAddExpence([FromBody] AddExpenceCommand request)
        {
            var isChanged = _mediator.Send(request).Result;

            return Ok(isChanged);
        }
    }
}