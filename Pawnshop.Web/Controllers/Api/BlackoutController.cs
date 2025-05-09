using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.BlackoutView)]
    public class BlackoutController : Controller
    {
        private readonly IDictionaryWithSearchService<Blackout, BlackoutFilter> _service;

        public BlackoutController(IDictionaryWithSearchService<Blackout, BlackoutFilter> service)
        {
            _service = service;
        }

        [HttpPost("/api/blackout/list")]
        public ListModel<Blackout> List([FromBody] ListQuery listQuery) => _service.List(listQuery);

        [HttpPost("/api/blackout/card")]
        public async Task<Blackout> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var blackout = await _service.GetAsync(id);
            if (blackout == null) throw new InvalidOperationException();

            return blackout;
        }

        [HttpPost("/api/blackout/save"), Authorize(Permissions.BlackoutManage)]
        [Event(EventCode.DictBlackoutSaved, EventMode = EventMode.Response)]
        public Blackout Save([FromBody] Blackout blackout)
        {
            ModelState.Validate();

            return _service.Save(blackout);
        }
    }
}