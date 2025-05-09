using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Integrations.Fcb;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Web.Engine;
using Pawnshop.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractView)]
    [Route("api/clientOtherPaymentsInfo")]
    public class ClientOtherPaymentsInfoController : Controller
    {
        private readonly IClientOtherPaymentsInfoService _clientOtherPaymentsInfoService;
        private readonly IStorage _storage;
        private readonly IFcb4Kdn _fcb;
        private readonly ISessionContext _sessionContext;

        public ClientOtherPaymentsInfoController(IClientOtherPaymentsInfoService clientOtherPaymentsInfoService,
            IStorage storage, IFcb4Kdn fcb, ISessionContext sessionContext)
        {
            _clientOtherPaymentsInfoService = clientOtherPaymentsInfoService;
            _storage = storage;
            _fcb = fcb;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        [Route("GetFcbContracts")]
        public async Task<IActionResult> GetFcbContracts([FromBody] FcbContractsRequest request)
        {
            request.FcbReportRequest.Author = _sessionContext.UserName;
            request.FcbReportRequest.AuthorId = _sessionContext.UserId;
            request.FcbReportRequest.OrganizationId = 1;
            request.FcbReportRequest.ReportType = Data.Models.Contracts.Kdn.FCBReportTypeCode.IndividualStandard;
            var report = await _fcb.GetReport(request.FcbReportRequest);
            var model = new ContractKdnDetailModel();
            if (report != null && report.AvailableReportsErrorResponse == null)
            {
                model = await _clientOtherPaymentsInfoService.GetClientOtherPaymentsModels(await _storage.Load(report.XmlLink, (ContainerName)Enum.Parse(typeof(ContainerName), report.FolderName)), request.ContractId, request.FcbReportRequest.ClientId, request.SubjectTypeId, _sessionContext.UserId, request.IsFromAdditionRequest);
                return Ok(model);
            }
            return BadRequest(report.AvailableReportsErrorResponse);
        }

        [HttpPost]
        [Route("GetExistingContracts")]
        public async Task<IActionResult> GetExistingContracts([FromBody] FcbContractsExistsRequest request)
        {
            var contracts = await _clientOtherPaymentsInfoService.GetExistingContracts(request);
            return Ok(contracts);
        }

        [HttpPost]
        [Route("UpdateFcbContract")]
        public async Task<IActionResult> UpdateFcbContract([FromBody] UpdateFcbContractRequest request)
        {
            await _clientOtherPaymentsInfoService.UpdateFcbContract(request, _sessionContext.UserId);
            return Ok();
        }

        /// <summary>
        /// Получает список платежей по кредитам заемщика и созаемщика
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("otherPaymentsList"), ProducesResponseType(typeof(List<ContractKdnDetailDto>), 200)]
        public IActionResult GetOtherPaymentsList([FromBody] int contractId)
        {
            if (contractId == default)
                throw new PawnshopApplicationException("Выберите клиента");   

            var clientOtherPayments = _clientOtherPaymentsInfoService.GetClientOtherPayments(contractId);
            return Ok(clientOtherPayments.Select(c => new ContractKdnDetailDto
            {
                Id = c.Id,
                ContractId = c.ContractId,
                ClientId = c.ClientId,
                SubjectTypeId = c.SubjectTypeId,
                MonthlyPaymentAmount = c.MonthlyPaymentAmount,
                OverdueAmount = c.OverdueAmount,
                FileRowId = c.FileRowId,
                FileRow = c.FileRow,
                AuthorId = c.AuthorId,
                CreditorName = c.CreditorName
            }));
        }

        /// <summary>
        /// Получает список платежей по кредитам заемщика и созаемщика
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("otherPaymentsModelsList"), ProducesResponseType(typeof(List<ContractKdnDetailDto>), 200)]
        public IActionResult GetOtherPaymentsModelList([FromBody] int contractId)
        {
            if (contractId == default)
                throw new PawnshopApplicationException("Создайте договор");

            var clientOtherPayments = _clientOtherPaymentsInfoService.GetClientOtherPaymentsModels(contractId);
            return Ok(clientOtherPayments);
        }
    }
}
