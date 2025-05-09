using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Insurance;
using Pawnshop.Web.Engine.Middleware;
using System.Threading.Tasks;
using Pawnshop.Services.Contracts;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Web.Controllers.Api
{
    public class InsurancePolicyController : Controller
    {
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly IInsuranceService _insuranceService;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;

        public InsurancePolicyController(
            IInsurancePoliceRequestService insurancePoliceRequestService,
            IInsuranceService insuranceService,
            IContractActionPrepaymentService contractActionPrepaymentService
            )
        {
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _insuranceService = insuranceService;
            _contractActionPrepaymentService = contractActionPrepaymentService;
        }

        [HttpPost]
        [Authorize(Permissions.ContractView)]
        [Event(EventCode.RegisterInsurancePolice, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> SendToInsuranceCompany([FromBody] int contractId)
        {
            var actualPoliceRequest = _insurancePoliceRequestService.GetNewPoliceRequest(contractId);

            if (actualPoliceRequest is null)
                throw new PawnshopApplicationException(@$"Нет активных заявок для регистрации страхового полиса по Договору с Id {contractId}");

            await _insuranceService.BPMRegisterPolicy(actualPoliceRequest);

            return Ok();
        }

        [HttpPost]
        [Event(EventCode.AcceptPolicy, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public IActionResult AcceptPolicy([FromBody] InsuranceIntegrationModel insuranceIntegration)
        {
            var policeRequest = _insurancePoliceRequestService.GetByGUID(insuranceIntegration.InsuranceIntegrationId);
            if (policeRequest == null)
                throw new PawnshopApplicationException($"По данному Guid = {insuranceIntegration.InsuranceIntegrationId} не найдено заявок на создания страхового полиса");

            _insuranceService.BPMAcceptPolicy(policeRequest, insuranceIntegration.InsuranceCompanyCode.Value);

            return Ok();
        }

        [HttpPost]
        [Event(EventCode.KillPolicy, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public IActionResult KillPolicy([FromBody] InsuranceIntegrationModel insuranceIntegration)
        {
            var policeRequest = _insurancePoliceRequestService.GetByGUID(insuranceIntegration.InsuranceIntegrationId);
            if (policeRequest == null)
                throw new PawnshopApplicationException($"По данному Guid = {insuranceIntegration.InsuranceIntegrationId} не найдено заявок на создания страхового полиса");

            var insurancePolicy = _insuranceService.BPMKillPolicy(policeRequest);
            if(insurancePolicy.Contract != null && insurancePolicy.Contract.Status == AccountingCore.Models.ContractStatus.Signed)
            {
                CreatePrepayment(insurancePolicy.Contract, insurancePolicy.InsurancePremium);
            }

            return Ok();
        }

        [HttpPost]
        [Authorize(Permissions.ContractView)]
        [Event(EventCode.CancelInsurancePolice, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public IActionResult CancelInsurance([FromBody] int contractId)
        {
            var actualPoliceRequest = _insurancePoliceRequestService.GetApprovedPoliceRequest(contractId) ?? _insurancePoliceRequestService.GetErrorPoliceRequest(contractId);
            
            if (actualPoliceRequest is null)
                throw new PawnshopApplicationException(@$"Нет подтвержденных заявок или заявок с ошибкой для регистрации страхового полиса по Договору с Id {contractId}");

            if (actualPoliceRequest.CreateDate.Date > DateTime.Now.Date)
                throw new PawnshopApplicationException($"Отменить страхование можно только в день создания. День создания: {actualPoliceRequest.CreateDate.Date.ToString("dd.MM.yyyy")}");

            _insuranceService.BPMCancelPolicy(actualPoliceRequest);

            return Ok();
        }

        [HttpPost]
        [Event(EventCode.AcceptCancelPolicy, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public IActionResult AcceptCancelPolicy([FromBody] InsuranceIntegrationModel insuranceIntegration)
        {
            var policeRequest = _insurancePoliceRequestService.GetByGUID(insuranceIntegration.InsuranceIntegrationId);
            if (policeRequest == null)
                throw new PawnshopApplicationException($"По данному Guid = {insuranceIntegration.InsuranceIntegrationId} не найдено заявок на аннулирования страхового полиса");

            _insuranceService.BPMAcceptCancelPolicy(policeRequest);

            return Ok();
        }

        [HttpPost]
        [Event(EventCode.KillCancelPolicy, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public IActionResult KillCancelPolicy([FromBody] InsuranceIntegrationModel insuranceIntegration)
        {
            var policeRequest = _insurancePoliceRequestService.GetByGUID(insuranceIntegration.InsuranceIntegrationId);
            if (policeRequest == null)
                throw new PawnshopApplicationException($"По данному Guid = {insuranceIntegration.InsuranceIntegrationId} не найдено заявок на аннулирования страхового полиса");

            _insuranceService.BPMKillCancelPolicy(policeRequest);

            return Ok();
        }

        private void CreatePrepayment(Contract contract, decimal insurancePremium)
        {
            _contractActionPrepaymentService.Exec
                (contract.Id, insurancePremium, 1
                , contract.BranchId,
                Constants.ADMINISTRATOR_IDENTITY, date: DateTime.Now, orderStatus: OrderStatus.Approved);
        }
    }
}