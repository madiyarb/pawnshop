using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Web.Engine;
using System.Collections.Generic;
using System;
using Pawnshop.Web.Models.List;
using Pawnshop.Data.Access;
using Pawnshop.Services.Collection.http;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/collection/history")]
    public class CollectionHistoryController : Controller
    {
        private readonly ICollectionHttpService<CollectionHistory> _httpService;
        private readonly UserRepository _userRepository;
        public CollectionHistoryController(ICollectionHttpService<CollectionHistory> httpService, UserRepository userRepository)
        {
            _httpService = httpService;
            _userRepository = userRepository;
        }

        [HttpPost("list"), ProducesResponseType(typeof(List<CollectionHistory>), 200)]
        public ListModel<CollectionHistory> List()
        {
            var list = _httpService.List().Result;
            list.ForEach(x => x.User = x.CreateUserId > 0 ? _userRepository.Get(x.CreateUserId) : null);
            
            return new ListModel<CollectionHistory>
            {
                List = list,
                Count = list.Count
            };
        }
        [HttpPost("save")]
        public IActionResult Save([FromBody] CollectionHistory item)
        {
            ModelState.Validate();
            int result = 0;
            var itemAction = item.Id.HasValue ? _httpService.Get(item.Id.Value.ToString()).Result : null;
            if (itemAction == null)
                result = _httpService.Create(item).Result;
            else if (item.Id.HasValue)
                result = _httpService.Update(item).Result;

            return result > 0 ? Ok(result) : throw new PawnshopApplicationException("Ошибка сервиса");
        }

        [HttpPost("card")]
        public IActionResult Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var result = _httpService.Get(id.ToString()).Result;
            ///if (result == null) throw new InvalidOperationException();

            result.User = _userRepository.Get(result.CreateUserId);

            return Ok(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var result = _httpService.Delete(id.ToString());

            return Ok(result);
        }

        [HttpGet("GetByContractId/{contractId}")]
        public IActionResult GetByContractId([FromRoute] int contractId)
        {
            if (contractId <= 0) throw new ArgumentOutOfRangeException(nameof(contractId));

            var list = _httpService.GetByContractId(contractId.ToString()).Result;
            list.ForEach(x => x.User = x.CreateUserId > 0 ? _userRepository.Get(x.CreateUserId) : null);

            return Ok(list);
        }
    }
}
