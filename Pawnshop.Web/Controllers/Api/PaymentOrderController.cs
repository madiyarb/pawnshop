using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Models.List;
using PaymentOrder = Pawnshop.AccountingCore.Models.PaymentOrder;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.PaymentOrderView)]
    public class PaymentOrderController : Controller
    {
        private readonly IDictionaryWithSearchService<PaymentOrder, PaymentOrderFilter> _service;
        private readonly ISessionContext _sessionContext;

        public PaymentOrderController(IDictionaryWithSearchService<PaymentOrder, PaymentOrderFilter> service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/PaymentOrder/list")]
        public ListModel<PaymentOrder> List([FromBody] ListQueryModel<PaymentOrderFilter> listQuery) => _service.List(listQuery);

        [HttpPost("/api/PaymentOrder/card")]
        public async Task<PaymentOrder> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/PaymentOrder/save"), Authorize(Permissions.PaymentOrderManage)]
        [Event(EventCode.DictPaymentOrderSaved, EventMode = EventMode.Response)]
        public PaymentOrder Save([FromBody] PaymentOrder model)
        {
            ModelState.Validate();

            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/PaymentOrder/delete"), Authorize(Permissions.PaymentOrderManage)]
        [Event(EventCode.DictPaymentOrderSaved, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}