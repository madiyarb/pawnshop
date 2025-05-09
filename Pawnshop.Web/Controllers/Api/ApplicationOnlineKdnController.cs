using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline.Kdn;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.PaymentSchedules;
using Pawnshop.Web.Models.ApplicationOnlineKdn;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/applicationOnline")]
    [ApiController]
    [Authorize]
    public class ApplicationOnlineKdnController : ControllerBase
    {
        private readonly ApplicationOnlineCarRepository _applicationOnlineCarRepository;
        private readonly ApplicationOnlineKdnLogRepository _applicationOnlineKdnLogRepository;
        private readonly ApplicationOnlineKdnPositionRepository _applicationOnlineKdnPositionRepository;
        private readonly IApplicationOnlineKdnService _applicationOnlineKdnService;
        private readonly ApplicationOnlinePositionRepository _applicationOnlinePositionRepository;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly ISessionContext _sessionContext;
        private readonly UserRepository _userRepository;

        public ApplicationOnlineKdnController(
            ApplicationOnlineCarRepository applicationOnlineCarRepository,
            ApplicationOnlineKdnLogRepository applicationOnlineKdnLogRepository,
            ApplicationOnlineKdnPositionRepository applicationOnlineKdnPositionRepository,
            IApplicationOnlineKdnService applicationOnlineKdnService,
            ApplicationOnlinePositionRepository applicationOnlinePositionRepository,
            ApplicationOnlineRepository applicationOnlineRepository,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            LoanPercentRepository loanPercentRepository,
            IPaymentScheduleService paymentScheduleService,
            ISessionContext sessionContext,
            UserRepository userRepository)
        {
            _applicationOnlineCarRepository = applicationOnlineCarRepository;
            _applicationOnlineKdnLogRepository = applicationOnlineKdnLogRepository;
            _applicationOnlineKdnPositionRepository = applicationOnlineKdnPositionRepository;
            _applicationOnlineKdnService = applicationOnlineKdnService;
            _applicationOnlinePositionRepository = applicationOnlinePositionRepository;
            _applicationOnlineRepository = applicationOnlineRepository;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _loanPercentRepository = loanPercentRepository;
            _paymentScheduleService = paymentScheduleService;
            _sessionContext = sessionContext;
            _userRepository = userRepository;
        }

        [HttpGet("{id}/kdn/calculate")]
        public ActionResult<ApplicationOnlineKdnLog> CalucateKdn([FromRoute] Guid id)
        {
            var author = _userRepository.Get(_sessionContext.UserId);

            var applicationOnline = _applicationOnlineRepository.Get(id);

            if (applicationOnline == null)
                return BadRequest($"Заявка [{id}] не найдена, расчет КДН невозможен! ");

            if (!applicationOnline.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {applicationOnline.ResponsibleManagerId} назначте заявку себя");
            }

            var virtualPaymentSchedule = CalculateVirtualPaymentSchedule(applicationOnline);

            if (virtualPaymentSchedule == null)
                return BadRequest($"Не удалось расчитать виртуальный график платежей для заявки {id}, проверьте корректность данных!");

            var result = _applicationOnlineKdnService.CalculateKdn(applicationOnline, virtualPaymentSchedule, author);

            if (result.IsStopCredit)
                return BadRequest($"По Заемщику имеется ограничение по получению информации в КБ. Статус \"STOP CREDIT\"!");

            return Ok(result);
        }

        [HttpGet("{id}/kdn/logs")]
        public ActionResult<IList<ApplicationOnlineKdnLogView>> GetLogs([FromRoute] Guid id)
        {
            var logs = _applicationOnlineKdnLogRepository.List(null, new { ApplicationOnlineId = id });

            var resultList = logs.Select(x => new ApplicationOnlineKdnLogView
            {
                ApplicationAmount = x.ApplicationAmount,
                ApplicationOnlineId = x.ApplicationOnlineId,
                ApplicationOnlineStatus = x.ApplicationOnlineStatus,
                ApplicationSettingId = x.ApplicationSettingId,
                ApplicationTerm = x.ApplicationTerm,
                AuthorId = x.AuthorId,
                AverageMonthlyPayment = x.AverageMonthlyPayment,
                ClientId = x.ClientId,
                CreateByName = x.Author?.Fullname,
                CreateDate = x.CreateDate,
                Id = x.Id,
                IncomeConfirmed = x.IncomeConfirmed,
                Kdn = x.Kdn,
                OtherPaymentsAmount = x.OtherPaymentsAmount,
                ResultText = x.ResultText,
                Success = x.Success,
                TotalIncome = x.TotalIncome,
            })
                .OrderByDescending(x => x.CreateDate);

            return Ok(resultList);
        }

        [HttpGet("{id}/kdn/positions")]
        public ActionResult<IList<ApplicationOnlineKdnPositionView>> GetPositions([FromRoute] Guid id)
        {
            var application = _applicationOnlineRepository.Get(id);

            if (application == null)
                return BadRequest($"Заявка {id} не найдена.");

            var list = _applicationOnlineKdnPositionRepository.List(null, new { ApplicationOnlineId = id });

            var result = list
                .Select(x => new ApplicationOnlineKdnPositionView
                {
                    ApplicationOnlineId = x.ApplicationOnlineId,
                    ClientId = x.ClientId,
                    CollateralType = x.CollateralType,
                    CreateDate = x.CreateDate,
                    DeleteDate = x.DeleteDate,
                    EstimatedCost = x.EstimatedCost,
                    Id = x.Id,
                    Name = x.Name,
                    ResultCost = Math.Round(x.EstimatedCost / 6m, 2),
                })
                .ToList();

            if (!result.Any())
            {
                var applicationPosition = _applicationOnlinePositionRepository.Get(application.ApplicationOnlinePositionId);
                var applicationCar = _applicationOnlineCarRepository.Get(application.ApplicationOnlinePositionId);

                result.Add(new ApplicationOnlineKdnPositionView
                {
                    ApplicationOnlineId = application.Id,
                    ClientId = application.ClientId,
                    CollateralType = applicationPosition.CollateralType,
                    CreateDate = applicationPosition.CreateDate,
                    EstimatedCost = applicationPosition.EstimatedCost ?? 0,
                    ResultCost = Math.Round((applicationPosition.EstimatedCost ?? 0M) / 6, 2),
                    Name = $"{applicationCar.TransportNumber} {applicationCar.Mark} {applicationCar.Model} {applicationCar.ReleaseYear}"
                });
            }

            return Ok(result);
        }


        private List<ContractPaymentSchedule> CalculateVirtualPaymentSchedule(ApplicationOnline application)
        {
            var product = _loanPercentRepository.Get(application.ProductId);

            if (product == null)
                return null;

            var maturityDate = DateTime.Now.AddMonths(application.LoanTerm);
            var applicationAmount = application.ApplicationAmount;

            // TODO: replace check insurance
            if (/*application.Insurance &&*/ product.InsuranceCompanies.Any())
            {
                var insuranceData = _insurancePremiumCalculator.GetInsuranceDataV2(applicationAmount, product.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId, product.Id);

                applicationAmount = insuranceData.InsurancePremium + (insuranceData.Eds == 0 || applicationAmount > 3909999 ? applicationAmount : insuranceData.Eds);
            }

            return _paymentScheduleService.Build(product.ScheduleType.Value, applicationAmount, product.LoanPercent, DateTime.Today, maturityDate, application.FirstPaymentDate);
        }
    }
}
