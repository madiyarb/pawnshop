using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Shared.Protocol;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.UKassa;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.UKassa;
using Pawnshop.Web.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants = Pawnshop.Core.Constants;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UKassaController : ControllerBase
    {
        private readonly UKassaAccountSettingsRepository _accountSettingsRepository;
        private readonly IUKassaReportsService _uKassaReportsService;
        private readonly IUKassaService _uKassaService;
        private readonly BranchContext _branchContext;
        private readonly UKassaRepository _uKassaRepository;
        private readonly UserRepository _userRepository;
        private readonly IUKassaHttpService _uKassaHttpService;
        private readonly ICashOrderService _cashOrderService;

        public UKassaController(UKassaAccountSettingsRepository accountSettingsRepository, IUKassaReportsService uKassaReportsService, IUKassaService uKassaService,
            BranchContext branchContext, UKassaRepository uKassaRepository, UserRepository userRepository, IUKassaHttpService uKassaHttpService,
            ICashOrderService cashOrderService)
        {
            _accountSettingsRepository = accountSettingsRepository;
            _uKassaReportsService = uKassaReportsService;
            _uKassaService = uKassaService;
            _branchContext = branchContext;
            _uKassaRepository = uKassaRepository;
            _userRepository = userRepository;
            _uKassaHttpService = uKassaHttpService;
            _cashOrderService = cashOrderService;
        }

        [HttpGet]
        [Route("Report")]
        public async Task<IActionResult> GetReport(int? shiftId, DateTime? date)
        {
            return Ok(await _uKassaReportsService.GetReport(_branchContext.Branch.Id, shiftId, date: date ?? DateTime.Now.Date));
        }

        [HttpGet]
        [Route("GetXZReport")]
        public async Task<IActionResult> GetXZReport(DateTime? date, int? shiftId)
        {
            if (date.HasValue && date == DateTime.Now.Date)
            {
                var kassaId = _uKassaRepository.GetKassaByBranch(_branchContext.Branch.Id);
                return Ok(_uKassaHttpService.GetXReport(kassaId));
            }
            else
            {
                if (!shiftId.HasValue) return BadRequest("Нужно выбрать смену");
                return Ok(_uKassaHttpService.GetZReport(shiftId.Value));
            }
        }

        [HttpPost]
        [Route("ReportOperations")]
        public async Task<IActionResult> GetReportOperations([FromBody] ListQueryModel<UKassaReportFilter> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<UKassaReportFilter>();
            if (listQuery.Model == null) listQuery.Model = new UKassaReportFilter();
            if (!listQuery.Model.BranchId.HasValue || listQuery.Model.BranchId.Value == 0)
            {
                listQuery.Model.BranchId = _branchContext.Branch.Id;
            }
            if (listQuery.Model.ReportDate.HasValue)
            {
                listQuery.Model.ReportDate = listQuery.Model.ReportDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return Ok(await _uKassaReportsService.GetOperations(_branchContext.Branch.Id, listQuery.Model.ShiftId, listQuery.Model.ReportDate.Value, listQuery.Page, listQuery.Filter, listQuery.Model.Status));
        }

        [HttpGet]
        [Route("GetShifts")]
        public async Task<IActionResult> GetShifts(DateTime? date)
        {
            return Ok(await _uKassaReportsService.GetShifts(_branchContext.Branch.Id, date: date ?? DateTime.Now.Date));
        }

        [HttpGet]
        [Route("AccountSettings/Get")]
        public async Task<IActionResult> GetAccountSettings([FromQuery]int AccountId)
        {
            return Ok(_accountSettingsRepository.Get(AccountId));
        }

        [HttpGet]
        [Route("AccountsSettings/GetAll")]
        public async Task<IActionResult> GetAllAccountSettings()
        {
            return Ok(_accountSettingsRepository.List(new ListQuery { }));
        }

        [HttpPost]
        [Route("AccountSettings/Add")]
        public async Task<IActionResult> AddAccountSettings([FromBody] UKassaAccountSettings model)
        {
            _accountSettingsRepository.Insert(model);
            return Ok(model);
        }

        [HttpPatch]
        [Route("AccountSettings/Update")]
        public async Task<IActionResult> UpdateAccountSettings([FromBody] UKassaAccountSettings model)
        {
            _accountSettingsRepository.Update(model);
            return Ok(model);
        }

        [HttpDelete]
        [Route("AccountSettings/Delete")]
        public async Task<IActionResult> DeleteAccountSettings([FromQuery] int AccountId)
        {
            _accountSettingsRepository.Delete(AccountId);
            return Ok();
        }

        [HttpPatch]
        [Route("CashOrder/Resend")]
        public async Task<IActionResult> Resend([FromQuery] int OrderId)
        {
            _uKassaService.ResendRequest(OrderId);
            return Ok();
        }

        [HttpGet]
        [Route("CashOrder/GetCheck")]
        public async Task<IActionResult> GetCheck([FromQuery] int OrderId)
        {
            var request = _uKassaRepository.GetByOrderId(OrderId);
            if (request != null)
            {
                var order = await _cashOrderService.GetAsync(OrderId);
                var kassir = _userRepository.Get(order.ApprovedId.HasValue ? order.ApprovedId.Value : order.AuthorId);
                    if(order.Language != null)
                    {
                        switch (order.Language.Code)
                        {
                            case Constants.KZ_LANGUAGE_CODE:
                                return Ok(JsonConvert.DeserializeObject<UKassaGenerateCheckResponse>(request.ResponseData).html_code_kz?.Replace("KASSIR", kassir.Fullname));
                                break;
                            case Constants.RU_LANGUAGE_CODE:
                                return Ok(JsonConvert.DeserializeObject<UKassaGenerateCheckResponse>(request.ResponseData).html_code?.Replace("KASSIR", kassir.Fullname));
                                break;
                            default:
                                Ok(JsonConvert.DeserializeObject<UKassaGenerateCheckResponse>(request.ResponseData).html_code?.Replace("KASSIR", kassir.Fullname));
                                break;
                        }

                    }
                return Ok(JsonConvert.DeserializeObject<UKassaGenerateCheckResponse>(request.ResponseData).html_code?.Replace("KASSIR", kassir.Fullname));
            }
            return NotFound();
        }
    }
}
