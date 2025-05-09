using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Insurance;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.InsuranceCompanySettingsView)]
    public class InsuranceCompanySettingController : Controller
    {
        private readonly IInsuranceCompanySettingService _insuranceCompanySettingService;
        private readonly ISessionContext _sessionContext;

        public InsuranceCompanySettingController(IInsuranceCompanySettingService insuranceCompanySettingService,
                                                 ISessionContext sessionContext)
        {
            _insuranceCompanySettingService = insuranceCompanySettingService;
            _sessionContext = sessionContext;
        }

        [HttpPost, Authorize(Permissions.InsuranceCompanySettingsManage)]
        public IActionResult Save([FromBody] LoanPercentSettingInsuranceCompany model)
        {
            return Ok(_insuranceCompanySettingService.Save(model, _sessionContext.UserId));
        }

        [HttpPost]
        public IActionResult List([FromBody] ListQueryModel<InsuranceSettingQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<InsuranceSettingQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new InsuranceSettingQueryModel();

            return Ok(_insuranceCompanySettingService.List(listQuery));
        }

        [HttpPost, Authorize(Permissions.InsuranceCompanySettingsManage)]
        public IActionResult Card([FromBody] int id)
        {
            return Ok(_insuranceCompanySettingService.Card(id));
        }

        [HttpPost]
        public IActionResult InsuranceCompanyList()
        {
            return Ok(_insuranceCompanySettingService.InsuranceCompaniesList());
        }
    }
}
