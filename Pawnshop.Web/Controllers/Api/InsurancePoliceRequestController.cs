using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Insurance;
using Pawnshop.Web.Models.Insurance;
using System;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Core;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class InsurancePoliceRequestController : Controller
    {
        private readonly IInsurancePoliceRequestService _service;
        private readonly IApplicationService _applicationService;
        private readonly ContractRepository _contractRepository;

        public InsurancePoliceRequestController(
            IInsurancePoliceRequestService service,
            IApplicationService applicationService,
            ContractRepository contractRepository)
        {
            _service = service;
            _applicationService = applicationService;
            _contractRepository = contractRepository;
        }

        [HttpPost]
        [Event(EventCode.InsuranceOnlineRequestOnAdditionSave, EventMode = EventMode.All, EntityType = EntityType.InsurancePolicy, IncludeFails = true)]
        public IActionResult SaveAdditionPoliceRequest([FromBody] InsurancePoliceRequest policeRequest)
        {
            if (policeRequest is null || policeRequest.RequestData is null || policeRequest.RequestData.AdditionCost == 0)
                throw new PawnshopApplicationException("Не заполнены все нужные поля");

            //для случаев, когда страхование не требуется по обычным клиентам (например сумма добора маленькая), убрали эту проверку
            //if (!_service.isPensioner(policeRequest) && policeRequest.RequestData.InsuranceAmount == 0)
            //    throw new PawnshopApplicationException("Не заполнена сумма для отправки в страховую компанию");

            Contract contract = _contractRepository.Get(policeRequest.ContractId);
			if (!policeRequest.IsInsuranceRequired && contract.Setting != null && !contract.Setting.IsInsuranceAvailable) {
	            return Ok(policeRequest);
			}
            _applicationService.ValidateApplicationForAdditionCost(policeRequest.ContractId, policeRequest.RequestData.AdditionCost);

            _service.FillRequest(policeRequest);

            policeRequest.AlgorithmVersion = policeRequest.RequestData.AlgorithmVersion;

            if (_service.isPensioner(policeRequest))
                policeRequest = _service.ChangeForPensioner(policeRequest);

            using (var transaction = _service.BeginTransaction())
            {
                if (policeRequest.Id > 0 && policeRequest.RequestData.InsurancePremium == 0)
                {
                    if(!policeRequest.IsInsuranceRequired)
                        _service.Save(policeRequest);
                    else
                    _service.Delete(policeRequest.Id);
                }

                if(_service.isPensioner(policeRequest))
                {
                    _service.DeleteInsurancePoliceRequestsByContractId(policeRequest.ContractId);
                }

                if (policeRequest.RequestData.InsurancePremium >= 0)
                   _service.Save(policeRequest);

                transaction.Commit();
            }

            return Ok(policeRequest);
        }

        [HttpPost]
        [Event(EventCode.InsurancePolicyRequestCalculation, EventMode = EventMode.All, EntityType = EntityType.InsurancePolicy, IncludeFails = true)]
        public IActionResult GetRequestData([FromBody] InsuranceRequestDataModel model) => Ok(_service.SetInsuranceRequestData(model));
       
        [HttpPost]
        public IActionResult CopyInsuranceRequest([FromBody] int contractId) => Ok(_service.CopyInsurancePoliceRequest(contractId));
    }
}