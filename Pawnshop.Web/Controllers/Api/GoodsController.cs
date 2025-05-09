using System;
using Microsoft.Data.SqlClient;
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
    [Authorize(Permissions.GoodsView)]
    public class GoodsController : Controller
    {
        private readonly GoodsRepository _repository;

        public GoodsController(GoodsRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<Position> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<Position>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public Position Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var position = _repository.Get(id);
            if (position == null) throw new InvalidOperationException();

            return position;
        }

        [HttpPost, Authorize(Permissions.GoodsManage)]
        [Event(EventCode.DictGoodSaved, EventMode = EventMode.Response)]
        public Position Save([FromBody] Position position)
        {
            ModelState.Validate();

            try
            {
                if (position.Id > 0)
                {
                    _repository.Update(position);
                }
                else
                {
                    _repository.Insert(position);
                }
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException("Поле наименование должно быть уникальным");
                }
                throw new PawnshopApplicationException(e.Message);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(e.Message);
            }

            return position;
        }

        [HttpPost, Authorize(Permissions.GoodsManage)]
        [Event(EventCode.DictGoodDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить позицию, так как она привязана к позиции договора");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}