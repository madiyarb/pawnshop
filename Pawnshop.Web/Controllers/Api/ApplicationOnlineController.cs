using KafkaFlow.Producers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Data.Models.ApplicationsOnline.Bindings;
using Pawnshop.Data.Models.ApplicationsOnline.Events;
using Pawnshop.Data.Models.ApplicationsOnline.Views;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ApplicationsOnlineCar;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation;
using Pawnshop.Data.Models.ApplicationsOnlinePosition;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.OnlineTasks;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.ApplicationOnlineRefinances;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.KFM;
using Pawnshop.Services.MaximumLoanTermDetermination;
using Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Extensions;
using Pawnshop.Web.Models.ApplicationOnline.FcbKdn;
using Pawnshop.Web.Models.ApplicationOnline;
using Pawnshop.Web.Models.Clients.Profiles;
using Pawnshop.Web.Models;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.ApplicationOnlineFiles;
using Pawnshop.Services.PaymentSchedules;
using Serilog;
using Pawnshop.Services.CreditLines;
using Pawnshop.Core.Utilities;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.TasOnlinePermissionValidator;
using Pawnshop.Services.ApplicationsOnline.ApplicationOnlineCreditLimitVerification;
using Pawnshop.Web.Engine;
using Pawnshop.Data.Models.Comments;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Services.TasCore;
using Pawnshop.Data.Models.ApplicationOnlineNpck;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class ApplicationOnlineController : Controller
    {
        private readonly IApplicationOnlineService _applicationOnlineService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractService _contractService;
        private readonly IProducerAccessor _producers;
        private readonly ISessionContext _sessionContext;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly ITasOnlinePermissionValidatorService _permissionValidator;
        private readonly ILogger _logger;
        private readonly BranchContext _branchContext;
        private readonly EnviromentAccessOptions _options;

        public ApplicationOnlineController(
            IApplicationOnlineService applicationOnlineService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractService contractService,
            IProducerAccessor producers,
            ISessionContext sessionContext,
            IPaymentScheduleService paymentScheduleService,
            ITasOnlinePermissionValidatorService permissionValidator,
            BranchContext branchContext,
            ILogger logger,
            IOptions<EnviromentAccessOptions> options)
        {
            _applicationOnlineService = applicationOnlineService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractService = contractService;
            _producers = producers;
            _sessionContext = sessionContext;
            _paymentScheduleService = paymentScheduleService;
            _logger = logger;
            _permissionValidator = permissionValidator;
            _branchContext = branchContext;
            _options = options.Value;
        }

        [HttpGet("applications/{id}")]
        [ProducesResponseType(typeof(ApplicationOnlineView), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetApplicationOnline(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ContractRepository contractRepository,
            [FromServices] ICreditLineService creditLineService)
        {
            var applicationView = await repository.GetView(id);

            if (applicationView == null)
                return NotFound();

            if (applicationView.CreditLineId.HasValue)
            {
                var upcomingPayments = contractRepository.GetUpcomingPaymentsDateByCreditLineId(applicationView.CreditLineId.Value);
                var maturityDate = contractRepository.GetOnlyContract(applicationView.CreditLineId.Value).MaturityDate;
                applicationView.MaxPeriodInMonths = creditLineService.GetRemainingPaymentsCount(applicationView.CreditLineId.Value, null);

                if (upcomingPayments.Any())
                    applicationView.CanEditFirstPaymentDate = false;
                else
                    applicationView.CanEditFirstPaymentDate = true;
            }
            else
            {
                applicationView.CanEditFirstPaymentDate = true;
            }

            return Ok(applicationView);
        }

        [HttpPost("applications/create")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateApplication(
            [FromBody] ApplicationOnlineCreateBinding binding,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] IInsurancePremiumCalculator insurancePremiumCalculator,
            [FromServices] IDomainService domainService,
            [FromServices] IClientQuestionnaireService clientQuestionnaireService,
            [FromServices] IClientAdditionalContactService clientAdditionalContactService,
            [FromServices] ApplicationOnlineCarRepository applicationOnlineCarRepository,
            [FromServices] ApplicationOnlinePositionRepository positionRepository,
            [FromServices] IMaximumLoanTermDeterminationService maximumLoanTermDeterminationService,
            [FromServices] ApplicationOnlineFileRepository applicationOnlineFileRepository,
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            [FromServices] ApplicationOnlineInsuranceRepository applicationOnlineInsuranceRepository,
            [FromServices] ClientContactRepository clientContactRepository,
            [FromServices] IApplicationOnlineRefinancesService applicationOnlineRefinancesService,
            [FromServices] VehicleLiquidityService vehicleLiquidityService,
            [FromServices] CreditLineRepository creditLineRepository,
            [FromServices] CarRepository carRepository,
            [FromServices] AttractionChannelRepository attractionChannelRepository)
        {
            try
            {
                Guid id = Guid.NewGuid();
                if (binding.ApplicationId != null)
                {
                    id = binding.ApplicationId.Value;
                }


                #region TryToSetLastClientContactToActual

                var contactType = domainService.GetDomainValue("CONTACT_TYPE", "MOBILE_PHONE");
                var clientContacts = clientContactRepository.List(new ListQuery(), new { ClientId = binding.ClientId })
                    .Where(contact => contact.ContactTypeId == contactType.Id);

                if (!clientContacts.Any())
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Client {binding.ClientId} haven't active number",
                        DRPPResponseStatusCode.CreationApplicationWIthoutNumber));
                }

                if (!clientContacts.Any(contact =>
                        contact.VerificationExpireDate != null && contact.VerificationExpireDate >= DateTime.Now))
                {

                    var clientContact = clientContacts.Where(contact => contact.IsDefault).OrderByDescending(contact => contact.CreateDate).FirstOrDefault();
                    if (clientContact != null)
                    {
                        if (clientContact.VerificationExpireDate == null ||
                            clientContact.VerificationExpireDate.Value.AddDays(3) < DateTime.Now)
                        {
                            clientContact.VerificationExpireDate = DateTime.Now.AddMonths(12);
                            clientContactRepository.Update(clientContact);
                        }
                    }
                    else
                    {
                        return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Client {binding.ClientId} haven't active number",
                            DRPPResponseStatusCode.CreationApplicationWIthoutNumber));
                    }
                }


                #endregion

                #region CreditLine
                if (binding.CreditLineId.HasValue)
                {
                    var creditLine = _contractService.GetOnlyContract(binding.CreditLineId.Value);

                    if (creditLine == null)
                        return BadRequest($"CreditLine {binding.CreditLineId} not found!");

                    var creditLineLimit = Math.Truncate(_contractService.GetCreditLineLimit(binding.CreditLineId.Value).Result);

                    if (binding.LoanCost > creditLineLimit)
                        return BadRequest($"The amount of tranche {binding.LoanCost} cannot be more than the amount of the credit line {creditLineLimit}!");

                    var maturityDate = DateTime.Today.AddMonths(binding.Period);

                    if (maturityDate > creditLine.MaturityDate)
                        return BadRequest($"The term of the tranche {maturityDate:yyyy-MM-dd} cannot be longer than the term of the credit line {creditLine.MaturityDate:yyyy-MM-dd}");
                }
                #endregion

                #region fill client additional contacts

                bool canFillQuestionnaire = clientQuestionnaireService.CanFillQuestionnaire(binding.ClientId);

                if (!canFillQuestionnaire)
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Client {binding.ClientId} cant fill Questionnaire",
                        DRPPResponseStatusCode.BadClient));

                var contactOwnerTypes = domainService.GetDomainValues(Constants.CONTACT_OWNER_TYPE_DOMAIN);

                if (binding.Contacts != null && binding.Contacts.Any())
                {
                    var contactsDto = binding.Contacts.Select(x =>
                            new ClientAdditionalContactDto
                            {
                                ContactOwnerFullname = $"{x.Lastname} {x.Firstname}",
                                ContactOwnerTypeId =
                                    contactOwnerTypes.FirstOrDefault(t => t.Code == x.RelationType).Id,
                                FromContactList = x.FromContactList,
                                PhoneNumber = x.Phone,
                            })
                        .ToList();

                    clientAdditionalContactService.SaveFromMobile(binding.ClientId, contactsDto);
                }

                #endregion

                string type = ApplicationOnlineType.BasicTranche.ToString();
                #region Car & Position
                ApplicationOnlineCar applicationOnlineCar = await
                    applicationOnlineCarRepository.GetExistedCarByBodyNumber(binding.Car.BodyNo);
                if (applicationOnlineCar != null)
                {
                    var createdOnlineCar = new ApplicationOnlineCar(binding.Car.Model, binding.Car.Category,
                        binding.Car.TechCategory,
                        binding.Car.IssueYear, binding.Car.GRNZ, binding.Car.EngineNo, binding.Car.BodyNo, binding.Car.SRTS,
                        binding.Car.StatusDate, binding.Car.Color, binding.Car.OwnerRegionName,
                        binding.Car.OwnerRegionNameEn, binding.Car.Notes);

                    if (!applicationOnlineCar.CarsEqualsForCreation(createdOnlineCar) && applicationOnlineCar.CarId.HasValue)
                    {
                        var finCoreCar = carRepository.Get(applicationOnlineCar.CarId.Value);
                        finCoreCar.MotorNumber = binding.Car.EngineNo;
                        finCoreCar.TechPassportNumber = binding.Car.SRTS;
                        finCoreCar.TechPassportDate = createdOnlineCar.TechPassportDate;
                        finCoreCar.Color = binding.Car.Color;
                        finCoreCar.TransportNumber = binding.Car.GRNZ;
                        carRepository.Update(finCoreCar);
                        applicationOnlineCar.MotorNumber = binding.Car.EngineNo;
                        applicationOnlineCar.TechPassportNumber = binding.Car.SRTS;
                        applicationOnlineCar.TechPassportDate = createdOnlineCar.TechPassportDate;
                        applicationOnlineCar.Color = binding.Car.Color;
                        applicationOnlineCar.TransportNumber = binding.Car.GRNZ;
                    }


                    applicationOnlineCar.FillExtensionFieldsFromMobileApp(binding.Car.Model, binding.Car.Category, binding.Car.TechCategory, binding.Car.EngineNo,
                        binding.Car.OwnerRegionName, binding.Car.OwnerRegionNameEn, binding.Car.Notes);

                    if (applicationOnlineCar.VehicleMarkId.HasValue && applicationOnlineCar.VehicleModelId.HasValue &&
                        applicationOnlineCar.ReleaseYear.HasValue)
                    {
                        var carLiquidity = vehicleLiquidityService.Get(applicationOnlineCar.VehicleMarkId.Value, applicationOnlineCar.VehicleModelId.Value, applicationOnlineCar.ReleaseYear.Value);

                        CarLiquidity liquidity;
                        switch (carLiquidity)
                        {
                            case 1:
                                liquidity = CarLiquidity.Low;
                                break;
                            case 2:
                                liquidity = CarLiquidity.Middle;
                                break;
                            case 3:
                                liquidity = CarLiquidity.High;
                                break;
                            default:
                                liquidity = CarLiquidity.Low;
                                break;
                        }

                        applicationOnlineCar.UpdateLiquidity(liquidity.ToString());
                    }
                    if (!binding.CreditLineId.HasValue)
                    {
                        if (await applicationOnlineRefinancesService.CreateRequiredRefinances(binding.Car.BodyNo, id))
                        {
                            type = ApplicationOnlineType.Refinance.ToString();
                        }
                    }
                    else
                    {
                        type = ApplicationOnlineType.AdditionalTranche.ToString();
                    }
                }
                else
                {
                    applicationOnlineCar = new ApplicationOnlineCar(binding.Car.Model, binding.Car.Category,
                        binding.Car.TechCategory,
                        binding.Car.IssueYear, binding.Car.GRNZ, binding.Car.EngineNo, binding.Car.BodyNo, binding.Car.SRTS,
                        binding.Car.StatusDate, binding.Car.Color, binding.Car.OwnerRegionName,
                        binding.Car.OwnerRegionNameEn, binding.Car.Notes);
                }
                await applicationOnlineCarRepository.Insert(applicationOnlineCar);
                var applicationOnlinePosition = new ApplicationOnlinePosition(applicationOnlineCar.Id);
                positionRepository.Insert(applicationOnlinePosition);
                #endregion

                #region Insurance
                if (binding.Insurance.HasValue && binding.Insurance.Value)
                {
                    var product = await _applicationOnlineService.GetProduct(binding.ProductId, true);
                    var calcInsurance = insurancePremiumCalculator.GetInsuranceDataV2(binding.LoanCost, product.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId, product.Id);
                    decimal amountForClient = calcInsurance.Eds == 0 || binding.LoanCost > 3909999 ? binding.LoanCost : calcInsurance.Eds;
                    binding.LoanCost += calcInsurance.InsurancePremium;

                    var insurance = new Data.Models.ApplicationOnlineInsurances.ApplicationOnlineInsurance(Guid.NewGuid(), id, calcInsurance.InsurancePremium, amountForClient, binding.LoanCost); // ToDO get From loanpercent settings

                    await applicationOnlineInsuranceRepository.Insert(insurance);
                }
                #endregion

                #region Application
                var maxLoanTerm = maximumLoanTermDeterminationService.Determinate(binding.ProductId);
                var signType = ApplicationOnlineSignType.SMS;
                var hasSingType = Enum.TryParse(_options.ApplicationOnlineSingType, out ApplicationOnlineSignType parsedSignType);

                if (hasSingType)
                    signType = parsedSignType;

                ApplicationOnline applicationOnline = new ApplicationOnline(id, binding.ClientId, binding.ProductId, binding.LoanCost,
                    binding.Period, signType, applicationOnlineCar.Id, binding.Insurance, binding.IncomeAmount, binding.ExpenseAmount, maxLoanTerm,
                    listId: binding.ListId, creditLineId: binding.CreditLineId, firstPaymentDate: binding.FirstPaymentDate, type: type, partnerCode: binding.PartnerCode);

                if (binding.CreditLineId != null)
                {
                    var attractionChannel =
                        await creditLineRepository.GetLastAttractionChannelId(binding.CreditLineId.Value);
                    if (attractionChannel != null)
                    {
                        applicationOnline.AttractionChannelId = attractionChannel;
                    }
                }
                else if (applicationOnline.AttractionChannelId == null || applicationOnline.AttractionChannelId == 0)
                {
                    var attractionChannel = attractionChannelRepository.GetIdByCode(Constants.DEFAULT_ATTRACTION_CHANNEL_CODE);
                    if (attractionChannel != 0)
                    {
                        applicationOnline.AttractionChannelId = attractionChannel;
                    }
                }
                var application = await repository.Insert(applicationOnline, User.GetUserId());

                #endregion

                #region Files

                if (binding.Files != null)
                {
                    foreach (var file in binding.Files)
                    {
                        var fileCode =
                            applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByBusinessType(file.Code);
                        Guid fileid = Guid.NewGuid();
                        await applicationOnlineFileRepository.Insert(new ApplicationOnlineFile(fileid, application.Id,
                            Guid.Parse(file.FileId), file.FileType, file.FileType, "",
                            () => this.UrlToAction<ApplicationOnlineFileController>(
                                nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                            fileCode.Id));
                    }
                }
                #endregion

                #region Kafka
                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);

                return Ok(new ApplicationOnlineCreationView
                {
                    Id = application.Id,
                    ApplicationNumber = application.ApplicationNumber
                });
                #endregion
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

        [HttpPost("applicationsonline/{id}/update")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> UpdateApplication(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineBinding binding,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] IMaximumLoanTermDeterminationService maximumLoanTermDeterminationService,
            [FromServices] ApplicationOnlineCarRepository carRepository,
            [FromServices] ContractRepository contractRepository,
            [FromServices] IPaymentScheduleService paymentScheduleService)
        {
            try
            {
                int authorId = _sessionContext.UserId;
                var applicationOnline = repository.Get(id);

                if (applicationOnline == null)
                {
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Заявка с идентификатором : {id} не найдена"));
                }

                if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(applicationOnline))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
                }

                if (applicationOnline.Status == ApplicationOnlineStatus.OnEstimation.ToString())
                {
                    var errors = new List<string>();

                    if (binding.ApplicationAmount.HasValue && binding.ApplicationAmount != applicationOnline.ApplicationAmount)
                        errors.Add("Нельзя изменять сумму заявки в текущем статусе.");

                    if (binding.ProductId.HasValue && binding.ProductId != applicationOnline.ProductId)
                        errors.Add("Нельзя изменять продукт заявки в текущем статусе.");

                    if (errors.Any())
                        return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, string.Join("//", errors.ToArray())));
                }

                if (!applicationOnline.CanEditing(_sessionContext.UserId))
                {
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Заявка принадлежит пользователю с id : {applicationOnline.ResponsibleManagerId} назначте заявку себя"));
                }

                if (binding.FirstPaymentDate.HasValue)
                {
                    var upcomingPayments = contractRepository.GetUpcomingPaymentsDateByCreditLineId(applicationOnline.CreditLineId ?? 0);

                    if (!upcomingPayments.Any())
                    {
                        paymentScheduleService.CheckPayDay(binding.FirstPaymentDate.Value.Day);
                    }
                }


                await _applicationOnlineService.ChangeDetailForInsurance(applicationOnline, Constants.ADMINISTRATOR_IDENTITY, binding.ProductId, binding.ApplicationAmount);

                applicationOnline.Update(binding.LoanTerm, binding.Stage, binding.ApplicationSource,
                    binding.LoanPurposeId, binding.BusinessLoanPurposeId,
                    binding.OkedForIndividualsPurposeId, binding.TargetPurposeId,
                    binding.AttractionChannelId,
                    binding.BranchId, authorId, firstPaymentDate: binding.FirstPaymentDate);

                if (binding.ProductId.HasValue)
                {
                    var car = carRepository.Get(applicationOnline.ApplicationOnlinePositionId);
                    MaximumLoanTermCarDeterminationModel carModel = null;

                    if (car != null)
                    {
                        if (car.VehicleMarkId != null && car.VehicleModelId != null && car.ReleaseYear != null)
                        {
                            carModel = new MaximumLoanTermCarDeterminationModel
                            {
                                CarMarkId = car.VehicleMarkId.Value,
                                CarModelId = car.VehicleModelId.Value,
                                ReleaseYear = car.ReleaseYear.Value
                            };
                        }
                    }

                    int maxLoanTerm = maximumLoanTermDeterminationService.Determinate(binding.ProductId.Value, carModel);
                    applicationOnline.SetMaximumAvailableLoanTerm(maxLoanTerm);
                }

                await repository.Update(applicationOnline);

                return NoContent();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse(HttpStatusCode.InternalServerError, exception.Message));
            }
        }

        [Authorize(Policy = Permissions.TasOnlineVerificator)]
        [HttpPost("applicationonline/{id}/approve")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> ApproveApplication(
            CancellationToken cancellationToken,
            [FromRoute] Guid id,
            [FromQuery] bool? passChecks,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ContractRepository contractRepository,
            [FromServices] IApplicationOnlineKdnService kdnService,
            [FromServices] IApplicationOnlineRefinancesService service,
            [FromServices] IAbsOnlineService absOnlineService,
            [FromServices] IApplicationOnlineCheckerService applicationOnlineCheckerService,
            [FromServices] IApplicationOnlineCarService applicationOnlineCarService,
            [FromServices] IApplicationOnlineFilesService applicationOnlineFilesService,
            [FromServices] ICreditLineService creditLineService,
            [FromServices] IApplicationOnlineCreditLimitVerificationService creditLimitVerificationService,
            [FromServices] IApplicationOnlineChecksService applicationOnlineChecksService)
        {
            try
            {
                var application = repository.Get(id);

                if (application == null)
                {
                    return NotFound();
                }

                if (application.Type == ApplicationOnlineType.Refinance.ToString())
                {
                    if (!await service.IsApplicationAmountMoreThenRefinancedSum(application))
                    {
                        return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                            "Сумма необходимая для рефинансирования превышает сумму заявки. " +
                            "Скорректируйте сумму заявки или уберите займы для рефинансирования"));
                    }
                }

                if (!(passChecks.HasValue && passChecks == true))
                {
                    var emptyFields = new List<string>();
                    emptyFields.AddRange(applicationOnlineCheckerService.ReadyForVerification(id));
                    emptyFields.AddRange(applicationOnlineCheckerService.ReadyForApprove(id));
                    if (emptyFields.Any())
                    {
                        return UnprocessableEntity(new ApplicationOnlineApproveEmptyFieldsProblem
                        {
                            EmptyFields = emptyFields,
                            Message = "Некоторые поля не заполнены",
                            Status = (int)HttpStatusCode.UnprocessableEntity
                        });
                    }
                }

                if (!applicationOnlineCheckerService.AgePermittedForInsurance(application.Id))
                {
                    return UnprocessableEntity(new BaseResponse(
                        HttpStatusCode.UnprocessableEntity,
                        $"Клиент пенсионер и займ не может быть выдан со страхованием!"));
                }

                var checkClientCoborrowerResult = await applicationOnlineCheckerService
                    .CheckClientCoborrowerAccountAmountLimitAsync(application);

                if (!checkClientCoborrowerResult.IsValid)
                {
                    return UnprocessableEntity(new BaseResponse(
                        HttpStatusCode.UnprocessableEntity,
                        checkClientCoborrowerResult.Message));
                }

                if (!application.CanEditing(_sessionContext.UserId))
                {
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                        $"Заявка принадлежит пользователю с id : {application.ResponsibleVerificatorId} назначте заявку себя"));
                }

                var IsCheckedChecks = applicationOnlineChecksService.IsCheckedChecks(id, application.Status);

                if (!IsCheckedChecks)
                {
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                        $"Не пройдены все проверки!"));
                }

                var checkKdnResult = kdnService.CheckCallCalcKdnToApprove(application);

                if (checkKdnResult.Any())
                {
                    var kdnErrorMessage = "Требуется расчет КДН!\r\n" + string.Join("\r\n", checkKdnResult.ToArray());
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                        kdnErrorMessage));
                }

                var carInfoActualized = await applicationOnlineCarService.ActualizeCarInfo(application.ApplicationOnlinePositionId, application.ClientId);

                if (!carInfoActualized)
                {
                    return UnprocessableEntity(new BaseResponse(HttpStatusCode.UnprocessableEntity,
                        "Нельзя оформить договор на автомобиль, который является залогом невыкупленого договора, другого клиента"));
                }

                var product = await _applicationOnlineService.GetProduct(application.ProductId);

                if (product.LoanCostFrom > application.ApplicationAmount)
                {
                    return UnprocessableEntity(new BaseResponse(HttpStatusCode.UnprocessableEntity,
                        $"Нельзя оформить договор так как сумма заявки меньше минимальной суммы, {application.ApplicationAmount} < {product.LoanCostFrom}."));
                }

                if (application.CreditLineId.HasValue)
                {
                    var creditLine = contractRepository.GetOnlyContract(application.CreditLineId.Value);

                    if (creditLine != null && creditLine.Status == ContractStatus.Signed)
                    {
                        var checkResult = await creditLineService.CheckForOpenTranche(creditLine.Id, creditLine);

                        if (!checkResult.IsCanOpen)
                        {
                            return UnprocessableEntity(new BaseResponse(HttpStatusCode.UnprocessableEntity, checkResult.Message));
                        }
                    }
                }

                var limitChecker = await creditLimitVerificationService.Check(application);

                if (!limitChecker.Result)
                {
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, limitChecker.Message));
                }

                using (var transaction = repository.BeginTransaction())
                {
                    application.ChangeStatus(ApplicationOnlineStatus.Approved, _sessionContext.UserId);

                    if (!CreateContract(application, application.ContractBranchId, out string error))
                    {
                        transaction.Rollback();

                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new BaseResponse(HttpStatusCode.InternalServerError, $"Не удалось сохранить контракт: {error}"));
                    }

                    absOnlineService.CreateInsurancePolicy(application.ContractId.Value);
                    await repository.Update(application);
                    transaction.Commit();
                }

                await service.SetContractIdForRefinancedItems(application);

                try
                {
                    Guid fileid = Guid.NewGuid();
                    var createLoanContractFileResult = applicationOnlineFilesService.CreateLoanContractFile(application, out string fileError,
                        () => this.UrlToAction<ApplicationOnlineFileController>(nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                        fileid, null, cancellationToken);

                    if (!createLoanContractFileResult)
                        throw new Exception($"Error generate loan document for application {id} : {fileError}");
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, exception.Message);
                }

                var updatedApplication = repository.Get(application.Id);
                ApplicationOnlineContractInfo contractInfo = new ApplicationOnlineContractInfo();
                if (updatedApplication.ContractId.HasValue)
                {
                    var contract = contractRepository.Get(updatedApplication.ContractId.Value);
                    contractInfo.MaturityDate = contract.MaturityDate;
                    var cps = contract.PaymentSchedule.OrderBy(psc => psc.Date).FirstOrDefault();
                    contractInfo.MonthlyPaymentAmount = cps?.DebtCost + cps?.PercentCost;
                }

                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString(),
                    ApplicationOnlineContractInfo = contractInfo
                };

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
            return NoContent();
        }

        [Authorize(Policy = Permissions.TasOnlineManager)]
        [HttpPost("applicationonline/{id}/toconsideration")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> ConsiderApplication(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository,
            CancellationToken cancellationToken)
        {
            try
            {
                var application = repository.Get(id);
                if (application == null)
                {
                    return NotFound();
                }

                if (application.ResponsibleManagerId != null)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new BaseResponse(HttpStatusCode.Forbidden, "У заявки уже есть владелец"));
                }

                if (application.Status == ApplicationOnlineStatus.Declined.ToString() ||
                    application.Status == ApplicationOnlineStatus.ContractConcluded.ToString())
                {
                    return BadRequest("Заявка находится в конечном статусе Договор заключен или Отклонена");
                }
                if (_branchContext.IsInitialized)
                {
                    if (_branchContext.Branch.IsTasOnlineBranchForApplicationOnline())
                        return StatusCode((int)HttpStatusCode.BadRequest,
                            new BaseResponse(HttpStatusCode.BadRequest, "Филиал пользователя это вторичный филиал TASONLINE. Выберите другой филиал."));
                }
                application.Considerate(_sessionContext.UserId, _branchContext.Branch.Id);
                await repository.Update(application);
                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };
                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
            return NoContent();
        }

        [Authorize(Policy = Permissions.TasOnlineManager)]
        [HttpPost("applicationonline/{id}/toverification")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> ToVerification(
            [FromRoute] Guid id,
            [FromQuery] bool? passChecks,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ClientsBlackListRepository clientlbakBlackListRepository,
            [FromServices] IApplicationOnlineKdnService kdnService,
            [FromServices] IKFMService kfmService,
            [FromServices] IApplicationOnlineCheckerService checkerService,
            [FromServices] IApplicationOnlineChecksService applicationOnlineChecksService,
            CancellationToken cancellationToken)
        {

            try
            {
                var application = repository.Get(id);

                if (application == null)
                {
                    return NotFound();
                }
                if (!(passChecks.HasValue && passChecks == true))
                {
                    var emptyFields = new List<string>();
                    emptyFields.AddRange(checkerService.ReadyForVerification(id));
                    if (emptyFields.Any())
                    {
                        return UnprocessableEntity(new ApplicationOnlineApproveEmptyFieldsProblem
                        {
                            EmptyFields = emptyFields,
                            Message = "Некоторые поля не заполнены",
                            Status = (int)HttpStatusCode.UnprocessableEntity
                        });
                    }

                }
                if (!application.CanEditing(_sessionContext.UserId))
                {
                    return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");
                }

                if (application.Status == ApplicationOnlineStatus.Declined.ToString() ||
                    application.Status == ApplicationOnlineStatus.ContractConcluded.ToString())
                {
                    return BadRequest("Заявка находится в конечном статусе Договор заключен или Отклонена");
                }

                var clientsInBlackList = clientlbakBlackListRepository.GetClientsBlackListsByClientId(application.ClientId);

                if (clientsInBlackList.Count > 0)
                {
                    return BadRequest("Невозможно отправить заявку на верификацию, клиент найден в черном списке.");
                }

                var kfmResult = await kfmService.FindByClientIdAsync(application.ClientId);

                if (kfmResult)
                {
                    return BadRequest("Невозможно отправить заявку на верификацию, клиент найден в списке КФМ.");
                }

                var isCheckedChecks = applicationOnlineChecksService.IsCheckedChecks(id, application.Status);

                if (!isCheckedChecks)
                {
                    return BadRequest("Не пройдены все проверки!");
                }

                var checkKdnResult = kdnService.CheckCallCalcKdnToVerification(application);

                if (checkKdnResult.Any())
                {
                    var kdnErrorMessage = "Требуется расчет КДН!\r\n" + string.Join("\r\n", checkKdnResult.ToArray());
                    return BadRequest(kdnErrorMessage);
                }

                if (application.BranchId == null && !_branchContext.IsInitialized)
                {
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                        "У заявки не установлен ContractBranchId и филиал не инициализирован"));
                }

                if (application.BranchId == null || _branchContext.Branch.Id != application.BranchId)
                {
                    application.SetContractBranch(_branchContext.Branch.Id);
                }

                application.ChangeStatus(ApplicationOnlineStatus.Verification, _sessionContext.UserId);
                await repository.Update(application);
                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
            return NoContent();
        }

        [HttpPost("applicationonline/{id}/reject")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> RejectApplication(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository,
            [FromBody] ApplicationOnlineRejectBinding binding,
            [FromServices] ApplicationOnlineRejectionReasonsRepository rejectionReasonsRepository,
            CancellationToken cancellationToken)
        {
            try
            {
                var application = repository.Get(id);

                if (application == null)
                {
                    return NotFound();
                }

                if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
                }

                if (application.Status == ApplicationOnlineStatus.Declined.ToString() ||
                    application.Status == ApplicationOnlineStatus.ContractConcluded.ToString())
                {
                    return BadRequest("Заявка находится в конечном статусе Договор заключен или Отклонена");
                }

                if (!application.CanEditing(_sessionContext.UserId))
                {
                    return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");
                }

                var rejectReason = await rejectionReasonsRepository.Get(binding.RejectionReasonId);

                if (rejectReason == null)
                {
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Такой причины отказа {binding.RejectionReasonId} не найдено"));
                }

                application.Reject(_sessionContext.UserId, binding.RejectionReasonId, rejectReason.Code, binding.Comment);
                await repository.Update(application);

                _applicationOnlineService.DeleteDraftContractEntities(application.ContractId);
                _applicationOnlineService.DeleteDraftContractEntities(application.CreditLineId);

                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }

            return NoContent();
        }

        [HttpPost("applicationonline/{id}/decline")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> RejectApplicationByClient(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ApplicationOnlineRejectionReasonsRepository rejectionReasonsRepository,
            [FromServices] IAbsOnlineService absOnlineService,
            [FromBody] ApplicationOnlineDeclineBinding binding,
            CancellationToken cancellationToken)
        {
            try
            {
                var rejectionReason = (await rejectionReasonsRepository.GetFiltredRejectionReasons(code: binding.ReasonCode)).List.SingleOrDefault();

                if (rejectionReason == null)
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"RejectionReason {binding.ReasonCode} not found",
                        DRPPResponseStatusCode.RejectionReasonNotFound));
                }

                var application = repository.Get(id);

                if (application == null)
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Application for {id} not found",
                        DRPPResponseStatusCode.ApplicationNotFound));
                }

                if (application.Status == ApplicationOnlineStatus.Declined.ToString() ||
                    application.Status == ApplicationOnlineStatus.ContractConcluded.ToString())
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Application in status {application.Status} and can't declined",
                        DRPPResponseStatusCode.ApplicationInWrongStatus));
                }

                application.Reject(_sessionContext.UserId, rejectionReason.Id, rejectionReason.Code,
                    $"~#Клиент отказался по причине : {binding.Reason}. Комментарий : #~ {binding.Comment}");
                await repository.Update(application);

                _applicationOnlineService.DeleteDraftContractEntities(application.ContractId);
                _applicationOnlineService.DeleteDraftContractEntities(application.CreditLineId);

                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);

                return NoContent();
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

        [HttpGet("appicalions/list")]
        public async Task<IActionResult> GetList(
            [FromServices] ApplicationOnlineRepository repository,
            [FromQuery] PageBinding pageBinding,
            [FromQuery] GetApplicationOnlineListBinding binding
            )
        {
            return Ok(await repository.GetList(binding, pageBinding.Offset, pageBinding.Limit, branchId: _branchContext.Branch.Id));
        }

        [HttpGet("estimationStatuses/list")]
        [ProducesResponseType(typeof(List<EnumView>), 200)]
        public async Task<IActionResult> GetEstimationStatusList()
        {
            List<EnumView> estimationStatuses = new List<EnumView>();

            foreach (ApplicationOnlineEstimationStatus status in Enum.GetValues(typeof(ApplicationOnlineEstimationStatus)))
            {
                estimationStatuses.Add(new EnumView
                {
                    Id = (int)status,
                    Name = status.ToString(),
                    DisplayName = status.GetDisplayName()
                });
            }

            return Ok(estimationStatuses);
        }

        [HttpGet("applicationOnlineStatuses/list")]
        [ProducesResponseType(typeof(List<EnumView>), 200)]
        public async Task<IActionResult> GetApplicationOnlineStatuses()
        {
            List<EnumView> estimationStatuses = new List<EnumView>();

            foreach (ApplicationOnlineStatus status in Enum.GetValues(typeof(ApplicationOnlineStatus)))
            {
                estimationStatuses.Add(new EnumView
                {
                    Id = (int)status,
                    Name = status.ToString(),
                    DisplayName = status.GetDisplayName()
                });
            }

            return Ok(estimationStatuses);
        }

        [HttpPatch("applications/{id}/branch")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> SetBranch(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineBranchBinding binding,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] GroupRepository groupRepository)
        {
            var applicationOnline = repository.Get(id);
            if (applicationOnline == null)
            {
                return NotFound($"Заявка с идентификатором : {id} не найдена");
            }

            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(applicationOnline))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            if (binding == null)
            {
                return NotFound($"Филиал обязателен к заполнению");
            }

            var branch = groupRepository.Get(binding.BranchId);
            if (branch == null)
            {
                return NotFound($"Филиал с идентификатором : {binding.BranchId} не найден");
            }

            applicationOnline.BranchId = branch.Id;
            await repository.Update(applicationOnline);

            return NoContent();
        }

        [HttpPost("applicationsonline/{id}/f2f")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> ToManualBiometricCheck(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineF2FBinding binding,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ApplicationOnlineFileRepository applicationOnlineFileRepository,
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            [FromServices] ContractRepository contractRepository,
            CancellationToken cancellationToken
            )
        {
            try
            {
                var applicationOnline = repository.Get(id);
                if (applicationOnline == null)
                {
                    return NotFound(new BaseResponseDRPP(HttpStatusCode.NotFound, $"ApplicationOnline with id : {id} not found",
                        DRPPResponseStatusCode.ApplicationNotFound));
                }

                var fileCode =
                    applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByCode("IdentityCard");

                Guid fileid = Guid.NewGuid();
                await applicationOnlineFileRepository.Insert(new ApplicationOnlineFile(fileid, applicationOnline.Id,
                    binding.DocFileId, binding.DocFileType, binding.DocFileType, "",
                    () => this.UrlToAction<ApplicationOnlineFileController>(
                        nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                    fileCode.Id, isAdditionalPhoto: true, sendToEstimate: false));

                fileCode = applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByCode("Liveness");

                fileid = Guid.NewGuid();
                await applicationOnlineFileRepository.Insert(new ApplicationOnlineFile(fileid, applicationOnline.Id,
                    binding.CamFileId, binding.CamFileType, binding.CamFileType, "",
                    () => this.UrlToAction<ApplicationOnlineFileController>(
                        nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                    fileCode.Id, isAdditionalPhoto: true, sendToEstimate: false));

                applicationOnline.ToBiometricCheck(_sessionContext.UserId, binding.Similarity);

                await repository.Update(applicationOnline);

                var updatedApplication = repository.Get(applicationOnline.Id);
                ApplicationOnlineContractInfo contractInfo = new ApplicationOnlineContractInfo();
                if (updatedApplication.ContractId.HasValue)
                {
                    var contract = contractRepository.Get(updatedApplication.ContractId.Value);
                    contractInfo.MaturityDate = contract.MaturityDate;
                    var cps = contract.PaymentSchedule.OrderBy(psc => psc.Date).FirstOrDefault();
                    contractInfo.MonthlyPaymentAmount = cps?.DebtCost + cps?.PercentCost;
                }
                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = applicationOnline,
                    Status = applicationOnline.Status.ToString(),
                    ApplicationOnlineContractInfo = contractInfo
                };
                await _producers["ApplicationOnline"]
                    .ProduceAsync(applicationOnline.Id.ToString(), message);
                return NoContent();
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

        [Authorize(Policy = Permissions.TasOnlineVerificator)]
        [HttpPost("applicationonline/{id}/passbiometric")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> PassBiometric(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ContractRepository contractRepository,
            [FromServices] IApplicationOnlineChecksService applicationOnlineChecksService)
        {
            try
            {
                var application = repository.Get(id);
                if (application == null)
                {
                    return NotFound();
                }

                if (application.Status == ApplicationOnlineStatus.Declined.ToString() ||
                    application.Status == ApplicationOnlineStatus.ContractConcluded.ToString())
                {
                    return BadRequest("Заявка находится в конечном статусе Договор заключен или Отклонена");
                }

                if (application.Status != ApplicationOnlineStatus.BiometricCheck.ToString())
                {
                    return BadRequest("Заявка не находится в статусе проверки биометрии");
                }
                application.ChangeStatus(ApplicationOnlineStatus.BiometricPassed, _sessionContext.UserId);
                await repository.Update(application);
                var updatedApplication = repository.Get(application.Id);

                applicationOnlineChecksService.ApproveF2FChecks(id, _sessionContext.UserId);

                ApplicationOnlineContractInfo contractInfo = new ApplicationOnlineContractInfo();
                if (updatedApplication.ContractId.HasValue)
                {
                    var contract = contractRepository.Get(updatedApplication.ContractId.Value);
                    contractInfo.MaturityDate = contract.MaturityDate;
                    var cps = contract.PaymentSchedule.OrderBy(psc => psc.Date).FirstOrDefault();
                    contractInfo.MonthlyPaymentAmount = cps?.DebtCost + cps?.PercentCost;
                }
                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString(),
                    ApplicationOnlineContractInfo = contractInfo
                };
                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
            return NoContent();
        }

        [Authorize]
        [HttpPost("applicationonline/{id}/changeResponsibleManager")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> ChangeResponsibleManager(
            [FromRoute] Guid id,
            [FromBody] ChangeResponsibleManagerBinding binding,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] OnlineTasksRepository onlineTasksRepository,
            [FromServices] UserRepository userRepository)
        {
            var application = repository.Get(id);
            if (application == null) { return NotFound(); }

            User user = null;
            if (application.ResponsibleManagerId.HasValue)
            {
                user = await userRepository.GetAsync(application.ResponsibleManagerId.Value);
            }
            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            if (!_permissionValidator.ManagerCanOwnApplication(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав. Либо заявка уже назначена"));
            }

            if (!_permissionValidator.IsAdministrator() && binding.UserId != null)
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "Недостаточно прав для переназначения пользователя кому либо"));
            }

            int? branchId = null;
            if (_branchContext.IsInitialized && binding.UserId == null)
            {
                if (_branchContext.Branch.IsTasOnlineBranchForApplicationOnline())
                    return StatusCode((int)HttpStatusCode.BadRequest,
                        new BaseResponse(HttpStatusCode.BadRequest, "Филиал пользователя это вторичный филиал TASONLINE. Выберите другой филиал."));
                branchId = _branchContext.Branch.Id;
            }

            binding.UserId ??= _sessionContext.UserId;
            application.ChangeResponsibleActor(binding.UserId.Value, branchId);
            var task = onlineTasksRepository.GetByApplicationId(id,
                new List<OnlineTaskStatus> { OnlineTaskStatus.Created, OnlineTaskStatus.Processing });
            if (task != null)
            {
                task.Processing(binding.UserId.Value);
                onlineTasksRepository.Update(task);
            }
            await repository.Update(application);

            return NoContent();
        }

        [Authorize(Policy = Permissions.TasOnlineAdministrator)]
        [HttpPost("applicationonline/{id}/changeadministrator")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> ChangeResponsibleAdministrator(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository)
        {
            var application = repository.Get(id);
            if (application == null)
                return NotFound();
            application.ResponsibleAdminId = _sessionContext.UserId;
            await repository.Update(application);
            return Ok();
        }

        [Authorize]
        [HttpDelete("applicationonline/{id}/changeResponsibleManager")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> DeleteResponsibleManager(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository)
        {
            var application = repository.Get(id);
            if (application == null) { return NotFound(); }

            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            if (!_permissionValidator.UserCanSetResponsibleManagerToNull(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "Заявка принадлежит другому пользователю"));
            }

            application.DeleteResponsibleManager();
            await repository.Update(application);
            return NoContent();
        }

        [HttpGet("applications/{id}/getmaximumexpireperiod")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetMaximumPeriod(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ClientExpiredSchedulesGetterService service)
        {
            var application = repository.Get(id);
            if (application == null) { return NotFound(); }

            return Ok(service.Calculate(application.ClientId));
        }

        [HttpGet("applications/{id}/expiredSchedules")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetClientExpiredSchedules(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ClientExpiredSchedulesGetterService service)
        {
            var application = repository.Get(id);
            if (application == null) { return NotFound(); }

            return Ok(service.GetAllExpiredPaymentSchedules(application.ClientId));
        }

        [HttpGet("creditline/{id}")]
        public async Task<ActionResult<ApplicationOnlineCreditLineInfo>> GetCreditLineInfo(
            [FromRoute] int id,
            [FromServices] AccountRepository _accountRepository,
            [FromServices] CarRepository _carRepository,
            [FromServices] LoanPercentRepository _loanPercentRepository,
            [FromServices] InscriptionRepository _inscriptionRepository
            )
        {
            var creditLine = _contractService.GetOnlyContract(id);

            if (creditLine == null)
                return NotFound($"СОКЛ {id} не найден.");

            var product = await _loanPercentRepository.GetOnlyAsync(id);
            var creditLineLimit = await _contractService.GetCreditLineLimit(id);

            var creditLineInfo = new ApplicationOnlineCreditLineInfo
            {
                Id = creditLine.Id,
                ContractNumber = creditLine.ContractNumber,
                CreateDate = creditLine.ContractDate,
                MaturityDate = creditLine.MaturityDate.Date,
                CreditLineLimit = creditLineLimit,
                LoanCost = creditLine.LoanCost,
                InscriptionStatus = "Взысканий нет",
            };

            var tranches = await _contractService.GetAllSignedTranches(id);

            if (tranches.Any())
            {
                var minPaymentDate = tranches.Min(t => t.NextPaymentDate);

                if (minPaymentDate != null && minPaymentDate < DateTime.Now)
                    creditLineInfo.PaymentExpiredDays = Math.Abs((DateTime.Now.Date - minPaymentDate.Value).Days);

                var tranchesBalance = _contractService.GetBalances(tranches.Select(x => x.Id).ToList());

                creditLineInfo.DebtLeft = tranchesBalance?.Sum(x => x.AccountAmount) ?? 0;
                creditLineInfo.CurrentDebt = tranchesBalance?.Sum(x => x.TotalRedemptionAmount) ?? 0;
            }

            var carInfo = await _carRepository.GetCarStatus(id);
            creditLineInfo.ParkingStatus = carInfo.Status;

            foreach (var tranche in tranches)
            {
                var inscriptions = await _inscriptionRepository.GetByContractId(tranche.Id);

                if (inscriptions.Any(x => x.Status == InscriptionStatus.Approved))
                {
                    creditLineInfo.InscriptionStatus = "Передан ЧСИ";
                    creditLineInfo.HasInscription = true;
                    break;
                }
            }

            return Ok(creditLineInfo);
        }

        [HttpGet("applicationonline/{id}/financeInfo")]
        public async Task<ActionResult<ApplicationOnlineFinanceInfo>> GetFinanceInfo(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository repository)
        {
            var application = repository.Get(id);

            if (application == null)
                return NotFound();

            var response = new ApplicationOnlineFinanceInfo
            {
                CorrectedExpenseAmount = application.CorrectedExpenseAmount ?? 0,
                CorrectedIncomeAmount = application.CorrectedIncomeAmount ?? 0,
                ExpenseAmount = application.ExpenseAmount ?? 0,
                IncomeAmount = application.IncomeAmount ?? 0,
            };

            return Ok(response);
        }

        [HttpPut("applicationonline/{id}/financeInfo")]
        public async Task<ActionResult<ApplicationOnlineFinanceInfo>> UpdateFinanceInfo(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineFinanceInfo financeInfo,
            [FromServices] ApplicationOnlineRepository repository)
        {
            var application = repository.Get(id);

            if (application == null)
                return NotFound();


            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            if (!application.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {application.ResponsibleManagerId} назначте заявку себя");
            }

            if (application.Status == ApplicationOnlineStatus.Declined.ToString() ||
                application.Status == ApplicationOnlineStatus.ContractConcluded.ToString())
            {
                return BadRequest("Заявка находится в конечном статусе Договор заключен или Отклонена");
            }

            application.CorrectedIncomeAmount = financeInfo.CorrectedIncomeAmount;
            application.CorrectedExpenseAmount = financeInfo.CorrectedExpenseAmount;

            await repository.Update(application);

            return Ok(financeInfo);
        }

        [HttpGet("applicationonline/{id}/fcbkdn/list")]
        public ActionResult<ApplicationOnlineFcbKdnListView> GetFcbdKdnList(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ApplicationOnlineFcbKdnPaymentRepository applicationOnlineFcbKdnPaymentRepository)
        {
            var applicationOnline = applicationOnlineRepository.Get(id);

            if (applicationOnline == null)
                return NotFound($"Заявка {id} не найдена!");

            var fcbList = applicationOnlineFcbKdnPaymentRepository.List(null, new { ApplicationOnlineId = id });

            var response = new ApplicationOnlineFcbKdnListView();

            response.List = fcbList?.Select(x =>
                new ApplicationOnlineFcbKdnView
                {
                    Id = x.Id,
                    CreateBy = x.CreateBy,
                    CreateByName = x.Author?.Fullname,
                    CreateDate = x.CreateDate,
                    PaymentAmount = x.PaymentAmount,
                    Success = x.Success
                })
                .ToList();

            return Ok(response);
        }

        [Authorize(Policy = Permissions.TasOnlineManager)]
        [HttpPost("applicationonline/{applicationId}/sign")]
        public async Task<IActionResult> SignContract(
            [FromRoute] Guid applicationId,
            [FromQuery] int? requisiteId,
            [FromQuery] bool? passChecks,
            [FromServices] IContractSigningService contractSigningService,
            [FromServices] ApplicationOnlineRepository repository,
            [FromServices] ContractRepository contractRepository,
            [FromServices] IApplicationOnlineCheckerService applicationOnlineCheckerService,
            [FromServices] IApplicationOnlineChecksService applicationOnlineChecksService,
            [FromServices] InnerNotificationRepository innerNotificationRepository,
            [FromServices] IApplicationOnlineFilesService applicationOnlineFilesService,
            CancellationToken cancellationToken)
        {
            try
            {
                var application = repository.Get(applicationId);
                if (application == null)
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                        $"Заявка с идентификатором {applicationId} не найдена"));
                if (!application.ContractId.HasValue)
                {
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                        $"Для заявки с идентификатором {applicationId} не создан договор"));
                }

                if (!(passChecks.HasValue && passChecks == true))
                {
                    var emptyFields = new List<string>();
                    emptyFields.AddRange(applicationOnlineCheckerService.ReadyForVerification(applicationId));
                    emptyFields.AddRange(applicationOnlineCheckerService.ReadyForApprove(applicationId));
                    emptyFields.AddRange(applicationOnlineCheckerService.ReadyForSign(applicationId));

                    if (emptyFields.Any())
                    {
                        return UnprocessableEntity(new ApplicationOnlineApproveEmptyFieldsProblem
                        {
                            EmptyFields = emptyFields,
                            Message = "Некоторые поля не заполнены",
                            Status = (int)HttpStatusCode.UnprocessableEntity
                        });
                    }
                }

                var IsCheckedChecks = applicationOnlineChecksService.IsCheckedChecks(applicationId, application.Status);

                if (!IsCheckedChecks)
                {
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                        $"Не пройдены все проверки!"));
                }

                await contractSigningService.SignTrancheAndCreditLine(application.ContractId.Value,
                    _sessionContext.UserId, application.ContractBranchId.Value, requisiteId, cashIssueBranchId: application.CashIssueBranchId);

                application.ChangeStatus(ApplicationOnlineStatus.ContractConcluded, _sessionContext.UserId);

                await repository.Update(application);

                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status,
                };
                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);

                var contract = contractRepository.GetOnlyContract(application.ContractId.Value);

                if (application.SignType == ApplicationOnlineSignType.SMS)
                {
                    try
                    {
                        Guid fileid = Guid.NewGuid();
                        var creationLoanFileResult = applicationOnlineFilesService.CreateLoanContractFile(application, out string fileError,
                            () => this.UrlToAction<ApplicationOnlineFileController>(nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                            fileid, null, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, exception.Message);
                    }
                }

                if (application.IsCashIssue && application.CashIssueBranchId.HasValue)
                {
                    innerNotificationRepository.Insert(new InnerNotification
                    {
                        CreateDate = DateTime.Now,
                        CreatedBy = Constants.ADMINISTRATOR_IDENTITY,
                        EntityType = EntityType.Contract,
                        EntityId = contract.Id,
                        Message = $"Ожидается выдача денежных средств через кассу филиала для онлайн договора {contract.ContractNumber}.",
                        ReceiveBranchId = application.CashIssueBranchId.Value,
                        Status = InnerNotificationStatus.Sent
                    });
                }

                return NoContent();
            }
            catch (ClientRequisiteNotFoundException exception)
            {
                _logger.Warning(exception, exception.Message);
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, exception.Message));
            }
            catch (ContractClassWrongException exception)
            {
                _logger.Error(exception, exception.Message);
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, exception.Message));
            }
            catch (ContractInWrongStatus exception)
            {
                _logger.Error(exception, exception.Message);
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, exception.Message));
            }
            catch (ContractNotFoundException exception)
            {
                _logger.Error(exception, exception.Message);
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, exception.Message));
            }
            catch (CreditLineNotFoundException exception)
            {
                _logger.Error(exception, exception.Message);
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, exception.Message));
            }
            catch (ContractSignFailedException exception)
            {
                _logger.Error(exception, exception.Message);
                return this.StatusCode((int)HttpStatusCode.BadRequest,
                    new BaseResponse(HttpStatusCode.InternalServerError, exception.Message));
            }
            catch (PayTypeNotAllowedException exception)
            {
                _logger.Error(exception, exception.Message);
                return NotFound(new BaseResponse(HttpStatusCode.BadRequest, exception.Message));
            }
            catch (ClientGeopositionNotActualException exception)
            {
                _logger.Warning(exception, exception.Message);
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, exception.Message));
            }
            catch (PawnshopApplicationException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponse(HttpStatusCode.InternalServerError, $"{exception.Message} "));
            }
            catch (TransferMoneyForRefinanceFailed exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponse(HttpStatusCode.InternalServerError, $"{exception.Message} "));
            }
            catch (NotEnoughMoneyRefinancing exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.UnprocessableEntity,
                    new BaseResponse(HttpStatusCode.UnprocessableEntity, $"{exception.Message} "));
            }
        }

        [Authorize(Policy = Permissions.TasOnlineAdministrator)]
        [HttpPost("movecontracttodraft/{id}")]
        public async Task<IActionResult> ToDraft([FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] PayOperationRepository payOperationRepository,
            [FromServices] ContractRepository contractRepository,
            [FromServices] ContractActionService contractActionService,
            [FromServices] CashOrderService cashOrderService,
            [FromServices] CashOrderRepository cashOrderRepository)
        {
            var applicationOnline = applicationOnlineRepository.Get(id);

            if (applicationOnline == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Заявка с {id} не найдена"));
            }

            if (!applicationOnline.ContractId.HasValue ||
                applicationOnline.Status != ApplicationOnlineStatus.ContractConcluded.ToString())
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                    $"Заявка не имеет значения ContractId либо не находиться в статусе договор заключен"));
            }
            var contract = contractRepository.Get(applicationOnline.ContractId.Value);

            if (!(contract.Status == ContractStatus.AwaitForMoneySend || contract.Status == ContractStatus.AwaitForOrderApprove))
            {
                return UnprocessableEntity(new BaseResponse(HttpStatusCode.UnprocessableEntity,
                    $"Договор не находиться в статусе \"Ожидает перечисления денег\" или \"Ожидает подтверждения кассового ордера\""));
            }

            var payOperationByContract = payOperationRepository.GetPayOperationByContractId(contract.Id);

            if (payOperationByContract != null)
            {
                var payOperation = payOperationRepository.Get(payOperationByContract.Id);
                if (payOperation != null)
                {
                    payOperationRepository.Delete(payOperation.Id);
                    ContractAction operactionAction = payOperation.Action;
                    if (payOperation.Action.ActionType == ContractActionType.Sign)
                    {
                        operactionAction.Status = ContractActionStatus.Canceled;
                        operactionAction.DeleteDate = DateTime.Now;
                        contractActionService.Save(operactionAction);
                    }
                    foreach (CashOrder order in cashOrderService.GetCashOrdersForApprove(payOperation.Orders))
                    {
                        cashOrderRepository.Delete(order.Id);
                    }
                }
            }

            contract.Status = ContractStatus.Draft;
            contractRepository.Update(contract);

            applicationOnline.Status = ApplicationOnlineStatus.ChangeRequisiteAfterContractConcluded.ToString();

            await applicationOnlineRepository.Update(applicationOnline);

            ApplicationOnlineContractInfo contractInfo = new ApplicationOnlineContractInfo();
            if (applicationOnline.ContractId.HasValue)
            {
                contractInfo.MaturityDate = contract.MaturityDate;
                var cps = contract.PaymentSchedule.OrderBy(psc => psc.Date).FirstOrDefault();
                contractInfo.MonthlyPaymentAmount = cps?.DebtCost + cps?.PercentCost;
            }

            var message = new ApplicationOnlineStatusChanged
            {
                ApplicationOnline = applicationOnline,
                Status = applicationOnline.Status.ToString(),
                ApplicationOnlineContractInfo = contractInfo
            };

            await _producers["ApplicationOnline"]
                .ProduceAsync(applicationOnline.Id.ToString(), message);

            return Ok();
        }

        [HttpPost("{id}/create-loan-signed-file")]
        public async Task<IActionResult> CreateLoanSignedFile(
            Guid id,
            CancellationToken cancellationToken,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ContractRepository contractRepository,
            [FromServices] ContractAdditionalInfoRepository contractAdditionalInfoRepository,
            [FromServices] ApplicationOnlineNpckFileRepository npckFileRepository,
            [FromServices] ApplicationOnlineFileRepository applicationOnlineFileRepository,
            [FromServices] IApplicationOnlineFilesService applicationOnlineFilesService)
        {
            var application = applicationOnlineRepository.Get(id);

            if (application == null)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Заявка {id} не найдена."));


            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            if (!application.ContractId.HasValue)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"У заявки {id} не создан контракт."));

            var contract = contractRepository.GetOnlyContract(application.ContractId.Value);

            if (contract == null)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найден договор {application.ContractId.Value}."));

            if (contract.Status == ContractStatus.Signed || contract.Status == ContractStatus.AwaitForMoneySend || contract.Status == ContractStatus.AwaitForOrderApprove)
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Договор {contract.Id} находится в недопустимом статусе создания файла."));

            var file = npckFileRepository.Find(new { ApplicationOnlineFileId = id });

            if (file != null)
            {
                var applicationFile = await applicationOnlineFileRepository.Get(file.ApplicationOnlineFileId);
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Имеется действующий файл, идентификатор файла [{applicationFile.StorageFileId}]"));
            }

            Guid fileid = Guid.NewGuid();
            var creationLoanFileResult = applicationOnlineFilesService.CreateLoanContractFile(application, out string fileError,
                () => this.UrlToAction<ApplicationOnlineFileController>(nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                fileid, null, cancellationToken);

            if (!creationLoanFileResult)
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Не удалось создать файл. {fileError}");

            return Ok(new BaseResponse(HttpStatusCode.OK, "Файл был создан"));
        }

        [HttpGet("linkbyadditionalcontact/{phonenumber}")]
        public async Task<ActionResult<AdditionalContactLink>> GetLinkApplicationsByAdditionalContactPhoneNumber(
            [FromRoute] string phonenumber,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository)
        {
            var editPhoneNumber = RegexUtilities.GetNumbers(phonenumber);

            if (!RegexUtilities.IsValidKazakhstanPhone(editPhoneNumber))
                return BadRequest($"Неверный номер телефона {phonenumber}");

            var linkList = await applicationOnlineRepository.GetLinkApplicationsByAdditionalContactPhoneNumber(editPhoneNumber);

            return Ok(linkList);
        }

        [HttpPost("applicationonline/{applicationId}/setcontractbranch/{branchid}")]
        public async Task<IActionResult> SetContractBranchId(
            [FromRoute] Guid applicationId,
            [FromRoute] int branchid,
            [FromServices] ApplicationOnlineRepository repository)
        {
            var application = repository.Get(applicationId);
            if (application == null)
                return NotFound();
            application.SetContractBranch(branchid);
            await repository.Update(application);
            return Ok();
        }

        [HttpGet("{clientid}/list")]
        public async Task<ActionResult<IEnumerable<ApplicationOnlineByClientIdView>>> GetListByClientId(
            [FromRoute] int clientid,
            [FromServices] ApplicationOnlineRepository repository)
        {
            var list = await repository.GetListByClientId(clientid);

            return Ok(list);
        }

        [Authorize(Policy = Permissions.TasOnlineVerificator)]
        [HttpPost("applicationonline/{id}/send-to-modification")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(BaseResponse), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> SendToModificationFromVerification(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineModificationFromVerificationBinding binding,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] CommentsRepository commentsRepository)
        {
            var application = applicationOnlineRepository.Get(id);
            if (application == null)
            {
                return NotFound();
            }

            if (!application.CanEditing(_sessionContext.UserId))
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                    $"Заявка принадлежит пользователю с id : {application.ResponsibleVerificatorId} назначте заявку себя"));
            }

            if (application.Status != ApplicationOnlineStatus.Verification.ToString())
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                    "Заявка должна быть в статусе Верификация!"));
            }

            if (binding == null || string.IsNullOrEmpty(binding.Comment))
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest,
                    "Комментарий обязателен к заполнению!"));
            }

            using (var transaction = applicationOnlineRepository.BeginTransaction())
            {
                application.ChangeStatus(ApplicationOnlineStatus.ModificationFromVerification, _sessionContext.UserId);

                await applicationOnlineRepository.Update(application);

                var entity = new Comment
                {
                    AuthorId = _sessionContext.UserId,
                    CommentText = binding.Comment,
                    ApplicationOnlineComment = new ApplicationOnlineComment
                    {
                        ApplicationOnlineId = id,
                        CommentType = ApplicationOnlineCommentTypes.Application,
                    }
                };

                commentsRepository.Insert(entity);

                transaction.Commit();

                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status.ToString()
                };

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), message);
            }

            return NoContent();
        }

        [HttpPost("{id}/cash-issue")]
        public async Task<IActionResult> SetCashIssueParameters(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineCashIssueBinding binding,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository)
        {
            var application = applicationOnlineRepository.Get(id);

            if (application == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Заявка {id} не найдена."));
            }

            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            try
            {
                application.IsCashIssue = binding.IsCashIssue;
                application.CashIssueBranchId = binding.CashIssueBranchId;
                application.LastChangeAuthorId = _sessionContext.UserId;
                application.UpdateDate = DateTime.Now;

                await applicationOnlineRepository.SetCashIssueBranch(application);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponse(HttpStatusCode.InternalServerError, $"Внутреняя ошибка сервиса: {ex.Message}."));
            }

            return NoContent();
        }

        [HttpGet("{id}/cash-issue")]
        public async Task<IActionResult> GetCashIssueParameters(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository)
        {
            var application = await applicationOnlineRepository.GetOnlyApplicationOnline(id);

            if (application == null)
                return NotFound();

            return Ok(new { application.IsCashIssue, application.CashIssueBranchId });
        }

        [HttpPost("{id}/npck/sign")]
        [ProducesResponseType(typeof(ApplicationOnlineNpckSignView), 200)]
        [ProducesResponseType(typeof(BaseResponseDRPP), 404)]
        [ProducesResponseType(typeof(BaseResponseDRPP), 500)]
        public async Task<IActionResult> NpckSign(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineNpckSignBinding binding,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ApplicationOnlineNpckFileRepository npckFileRepository,
            [FromServices] ApplicationOnlineNpckSignRepository npckSignRepository,
            [FromServices] ClientRepository clientRepository,
            [FromServices] IApplicationOnlineFilesService applicationOnlineFilesService,
            [FromServices] ITasCoreNpckService tasCoreNpckService,
            [FromServices] IClientContactService clientContactService,
            CancellationToken cancellationToken)
        {
            try
            {
                var application = applicationOnlineRepository.Get(id);

                if (application == null)
                    return NotFound(new BaseResponseDRPP(HttpStatusCode.NotFound, $"The application {id} not found!", DRPPResponseStatusCode.ApplicationNotFound));

                if (application.SignType != ApplicationOnlineSignType.NPCK)
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new BaseResponseDRPP(HttpStatusCode.InternalServerError, "Method not active!",
                            DRPPResponseStatusCode.UnspecifiedProblem));

                var signEntity = npckSignRepository.Find(new { ApplicationOnlineId = id, CheckExpireDate = true });

                if (signEntity != null)
                    return Ok(new ApplicationOnlineNpckSignView { Url = signEntity.SignUrl });

                var loanFile = npckFileRepository.Find(new { ApplicationOnlineId = id });

                if (loanFile == null)
                {
                    try
                    {
                        Guid fileid = Guid.NewGuid();
                        var createLoanContractFileResult = applicationOnlineFilesService.CreateLoanContractFile(application, out string fileError,
                            () => this.UrlToAction<ApplicationOnlineFileController>(nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                            fileid, binding.Language, cancellationToken, true);

                        if (!createLoanContractFileResult)
                            throw new Exception($"Error generate loan document for application {id} : {fileError}");

                        loanFile = npckFileRepository.Find(new { ApplicationOnlineId = id });
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, exception.Message);
                        return StatusCode((int)HttpStatusCode.InternalServerError,
                            new BaseResponseDRPP(HttpStatusCode.InternalServerError, exception.Message, DRPPResponseStatusCode.UnspecifiedProblem));
                    }
                }

                var client = clientRepository.GetOnlyClient(application.ClientId);
                var clientContacts = clientContactService.GetMobilePhoneContacts(client.Id);
                var mobilePhone = clientContacts.FirstOrDefault(x => x.IsDefault).Address;

                var npckSignResult = await tasCoreNpckService.GenerateUrl(client.IdentityNumber, mobilePhone, binding.RedirectUri, binding.Language, new List<Guid> { loanFile.NpckFileId }, cancellationToken);

                if (!npckSignResult.Success)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new BaseResponseDRPP(HttpStatusCode.InternalServerError, $"Error generate url for application {id} : {npckSignResult.Message}", DRPPResponseStatusCode.UnspecifiedProblem));
                }

                signEntity = new ApplicationOnlineNpckSign(id, npckSignResult.SignUrl);

                npckSignRepository.Insert(signEntity);

                return Ok(new ApplicationOnlineNpckSignView { Url = npckSignResult.SignUrl });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, ex.Message, DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }

        [HttpPost("{id}/npck/code")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(BaseResponseDRPP), 404)]
        [ProducesResponseType(typeof(BaseResponseDRPP), 500)]
        public async Task<IActionResult> NpckCodeValidateToToken(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineNpckSaveSignedFileBinding binding,
            [FromServices] ApplicationOnlineNpckFileRepository applicationOnlineNpckFileRepository,
            [FromServices] ApplicationOnlineNpckSignRepository applicationOnlineNpckSignRepository,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ITasCoreNpckService tasCoreNpckService,
            CancellationToken cancellationToken)
        {
            try
            {
                var application = applicationOnlineRepository.Get(id);

                if (application == null)
                    return NotFound(new BaseResponseDRPP(HttpStatusCode.NotFound, $"The application {id} not found!", DRPPResponseStatusCode.ApplicationNotFound));

                if (application.SignType != ApplicationOnlineSignType.NPCK)
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new BaseResponseDRPP(HttpStatusCode.InternalServerError, "Method not active!",
                            DRPPResponseStatusCode.UnspecifiedProblem));

                var signEntity = applicationOnlineNpckSignRepository.Find(new { ApplicationOnlineId = id });

                if (signEntity == null)
                    return NotFound(new BaseResponseDRPP(HttpStatusCode.NotFound, $"The application {id} not found signed uri!", DRPPResponseStatusCode.UnspecifiedProblem));

                signEntity.Code = binding.Code;
                applicationOnlineNpckSignRepository.Update(signEntity);

                var npckSignResult = await tasCoreNpckService.GetToken(id, binding.Code, binding.RedirectUri, cancellationToken);

                if (!npckSignResult.Success)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new BaseResponseDRPP(HttpStatusCode.InternalServerError, $"Error get token for application {id} : {npckSignResult.Message}", DRPPResponseStatusCode.UnspecifiedProblem));
                }

                signEntity.Sign();
                applicationOnlineNpckSignRepository.Update(signEntity);

                application.ChangeStatus(ApplicationOnlineStatus.RequisiteCheck, _sessionContext.UserId);
                await applicationOnlineRepository.Update(application);

                var messageChangeStatus = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = application,
                    Status = application.Status
                };

                await _producers["ApplicationOnline"]
                    .ProduceAsync(application.Id.ToString(), messageChangeStatus);

                var documents = applicationOnlineNpckFileRepository.List(null, new { ApplicationOnlineId = id });
                var filesInfo = documents.Where(x => x.FutureFileStorageId.HasValue).Select(x => new ApplicationOnlineNpckFileView(x.NpckFileId, x.FutureFileStorageId.Value)).ToList();

                var message = new ApplicationOnlineNpckEsignDocument
                {
                    ApplicationOnlineId = id,
                    ListId = application.ListId.Value,
                    Token = npckSignResult.Token,
                    FilesInfo = filesInfo
                };

                await _producers["NpckEsignDocument"]
                    .ProduceAsync(application.Id.ToString(), message);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, ex.Message, DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }


        // TODO: костыль для работы с IContractScheduleBuilder
        private bool CreateContract(ApplicationOnline applicationOnline, int? branchId, out string error)
        {
            error = string.Empty;

            try
            {
                _applicationOnlineService.CreateContract(applicationOnline, branchId).Wait();

                var creditLine = _contractService.Get(applicationOnline.CreditLineId.Value);

                var tranche = _contractService.Get(applicationOnline.ContractId.Value);
                SetAnyContractParameters(tranche).Wait();

                if (_contractPaymentScheduleService.IsNeedUpdatePaymentSchedule(tranche.PaymentSchedule, tranche.Id))
                    _contractPaymentScheduleService.Save(tranche.PaymentSchedule, tranche.Id, 1);

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private async Task SetAnyContractParameters(Contract contract, bool isFirstTranche = false)
        {
            if (contract.Status != AccountingCore.Models.ContractStatus.Draft)
                return;

            _paymentScheduleService.BuildWithContract(contract);

            if (!contract.FirstPaymentDate.HasValue && isFirstTranche)
                contract.FirstPaymentDate = contract.PaymentSchedule.FirstOrDefault().Date;

            contract.NextPaymentDate = contract.PaymentSchedule.FirstOrDefault().Date;
            contract.AnnuityType = _contractService.GetAnnuityType(contract);
            await _contractService.CalculateAPR(contract);

            _contractService.Save(contract);
        }
    }
}
