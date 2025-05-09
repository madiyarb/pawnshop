using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.AttractionChannelView)]
    public class AttractionChannelController : Controller
    {
        private readonly AttractionChannelRepository _repository;

        public AttractionChannelController(AttractionChannelRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("/api/attractionChannel/list")]
        public ListModel<AttractionChannel> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<AttractionChannel>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost("/api/attractionChannel/card")]
        public AttractionChannel Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var channel = _repository.Get(id);
            if (channel == null) throw new InvalidOperationException();

            return channel;
        }

        [HttpPost("/api/attractionChannel/save"), Authorize(Permissions.AttractionChannelManage)]
        [Event(EventCode.DictAttractionChannelSaved, EventMode = EventMode.Response)]
        public AttractionChannel Save([FromBody] AttractionChannel channel)
        {
            ModelState.Validate();

            if (channel.Id > 0)
            {
                _repository.Update(channel);
            }
            else
            {
                _repository.Insert(channel);
            }
            return channel;
        }

        [HttpPost("/api/attractionChannel/delete"), Authorize(Permissions.AttractionChannelManage)]
        [Event(EventCode.DictAttractionChannelDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}