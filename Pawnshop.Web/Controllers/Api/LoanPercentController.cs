using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Base;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.LoanPercent;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.List;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Web.Controllers.Api
{
    public class LoanPercentController : Controller
    {
        private readonly LoanPercentRepository _repository;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly ILoanPercentService _loanPercentService;

        public LoanPercentController(
            LoanPercentRepository repository,
            ISessionContext sessionContext,
            BranchContext branchContext,
            ILoanPercentService loanPercentService
            )
        {
            _repository = repository;
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _loanPercentService = loanPercentService;
        }

        [HttpPost]
        public ListModel<LoanPercentSetting> List([FromBody] ListQueryModel<LoanPercentQueryModel> listQuery)
        {
            if (listQuery.Model == null)
            {
                listQuery.Model = new LoanPercentQueryModel
                {
                    OrganizationId = _sessionContext.OrganizationId,
                };
            }
            else
            {
                listQuery.Model.OrganizationId = _sessionContext.OrganizationId;
            }

            return new ListModel<LoanPercentSetting>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public LoanPercentSetting Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var setting = _repository.Get(id);
            if (setting == null) throw new InvalidOperationException();

            return setting;
        }

        [HttpPost]
        public LoanPercentSetting Find([FromBody] LoanPercentQueryModel query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            query.BranchId = _branchContext.Branch.Id;

            var loanPercentSetting = _repository.Find(query);

            if (loanPercentSetting is null)
            {
                query.LoanCost = null;
                loanPercentSetting = _repository.Find(query);
            }

            return loanPercentSetting;
        }

        [HttpPost, Authorize(Permissions.LoanPercentSettingManage)]
        [Event(EventCode.DictLoanPercentSaved, EventMode = EventMode.Response)]
        public LoanPercentSetting Save([FromBody] LoanPercentSetting setting)
        {
            if (setting.Id == 0)
            {
                setting.OrganizationId = _sessionContext.OrganizationId;
            }

            ModelState.Clear();
            TryValidateModel(setting);
            ModelState.Validate();

            if (setting.IsProduct)
            {
                if (setting.ProductType == null) throw new PawnshopApplicationException("Не найден вид продукта");
                if (setting.ProductType.CollateralType != setting.CollateralType) throw new PawnshopApplicationException("Вид залога в продукте и в виде продукта отличаются");

                if (!setting.ContractPeriodFrom.HasValue || !setting.ContractPeriodFromType.HasValue) throw new PawnshopApplicationException("Ошибка срока кредитования");
                if (setting.ContractPeriodFrom.HasValue && setting.ContractPeriodFrom <= 0) throw new PawnshopApplicationException("Cрок кредитования не может быть меньше 1");

                if (!setting.ContractPeriodTo.HasValue || !setting.ContractPeriodToType.HasValue) throw new PawnshopApplicationException("Ошибка срока кредитования");
                if (setting.ContractPeriodTo.HasValue && setting.ContractPeriodTo <= 0) throw new PawnshopApplicationException("Cрок кредитования не может быть меньше 1");

                if ((setting.ContractPeriodFrom.HasValue && setting.ContractPeriodTo.HasValue) && (setting.ContractPeriodFrom * (int)setting.ContractPeriodFromType) > (setting.ContractPeriodTo * (int)setting.ContractPeriodToType)) throw new PawnshopApplicationException("Cрок кредитования не может быть отрицательным, проверьте поля скоров кредитования");

                if (!setting.DebtPeriod.HasValue || !setting.DebtPeriodType.HasValue) throw new PawnshopApplicationException("Ошибка периода погашения основного долга");
                if (setting.DebtPeriod.HasValue && setting.DebtPeriod <= 0) throw new PawnshopApplicationException("Период погашения основного долга не может быть меньше 1");

                if (!setting.PaymentPeriod.HasValue || !setting.PaymentPeriodType.HasValue) throw new PawnshopApplicationException("Ошибка срока погашения процентов");
                if (setting.PaymentPeriod.HasValue && setting.PaymentPeriod <= 0) throw new PawnshopApplicationException("Cрок погашения процентов не может быть меньше 1");

                if (setting.Restrictions != null && setting.Restrictions.Count > 0)
                {
                    setting.LoanCostFrom = setting.Restrictions.Min(x => x.LoanCostFrom);
                    setting.LoanCostTo = setting.Restrictions.Max(x => x.LoanCostTo);
                    //TODO: переделать при реализации restriction.HasPeriods
                    //foreach (var restriction in setting.Restrictions)
                    //{
                    //    if (restriction.HasPeriods && (restriction.Periods == null || !(restriction.Periods.Count > 0)))
                    //        throw new PawnshopApplicationException($"В периоде {restriction.LoanCostFrom} - {restriction.LoanCostTo} не найдены настройки периодов");
                    //    if (restriction.HasPeriods && (restriction.Periods.Count != setting.PaymentCount()))
                    //        throw new PawnshopApplicationException($"В периоде {restriction.LoanCostFrom} - {restriction.LoanCostTo} некорректное количество периодов (должно быть {setting.PaymentCount()}, найдено {restriction.Periods.Count})");
                    //}
                }
            }

            if (setting.Id > 0)
            {
                _repository.Update(setting);
            }
            else
            {
                _repository.Insert(setting);
            }

            return setting;
        }

        [HttpPost, Authorize(Permissions.LoanPercentSettingManage)]
        [Event(EventCode.DictLoanPercentDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }

        [HttpGet("/api/loanpercent/getchildsettings/{id}")]
        [Event(EventCode.DictLoanPercentDeleted, EventMode = EventMode.Request)]
        public async Task<IActionResult> GetChildSettings(int id)
        {
            var childs = await _repository.GetChild(id);

            return Ok(childs.Where(x => x.UseSystemType != UseSystemType.ONLINE && x.IsActual));
        }

        [HttpGet("/api/loanpercent/from-mobile")]
        public async Task<IActionResult> GetListFromMobile([FromQuery] string productTypeCode)
        {
            try
            {
                if (string.IsNullOrEmpty(productTypeCode))
                    return BadRequest("Код тип продуктов не может быть пустым.");

                return Ok(await _loanPercentService.GetListFromMobile(productTypeCode));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("/api/loanpercent/list/online")]
        public ActionResult<IList<LoanPercentOnlineView>> GetListForOnline([FromQuery] ContractClass contractClass = ContractClass.Tranche)
        {
            var productList = _loanPercentService.GetListForOnline(contractClass);
            return Ok(productList);
        }
    }
}