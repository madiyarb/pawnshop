using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.BranchConfigurationManage)]
    public class RemittanceSettingController : Controller
    {
        //private readonly RemittanceSettingRepository _repository;
        private readonly IDictionaryWithSearchService<RemittanceSetting, RemittanceSettingFilter> _service;

        public RemittanceSettingController(IDictionaryWithSearchService<RemittanceSetting, RemittanceSettingFilter> service)
        {
            _service = service;
        }

        [HttpPost]
        public ListModel<RemittanceSetting> List([FromBody] ListQueryModel<RemittanceSettingFilter> listQuery) => _service.List(listQuery);

        [HttpPost]
        public async Task<RemittanceSetting> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost]
        public RemittanceSetting Save([FromBody] RemittanceSetting model)
        {
            ModelState.Validate();

            return _service.Save(model);
        }

        [HttpPost]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);
            return Ok();
        }
    }
}
