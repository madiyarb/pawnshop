using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Contracts;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractView), Route("api/contractDiscount")]
    public class ContractDiscountController : Controller
    {
        private readonly ContractDiscountRepository _repository;
        private readonly ISessionContext _sessionContext;
        private readonly IContractDiscountService _contractDiscountService;

        public ContractDiscountController(ContractDiscountRepository repository,
            ISessionContext sessionContext,
            IContractDiscountService contractDiscountService)
        {
            _repository = repository;
            _sessionContext = sessionContext;
            _contractDiscountService = contractDiscountService;
        }

        [HttpPost("{id:int}/card"), ProducesResponseType(typeof(ContractDiscount), 200)]
        public IActionResult Card([FromRoute] int id)
        {
            ContractDiscount contractDiscount = _contractDiscountService.Get(id);
            return Ok(contractDiscount);
        }

        [HttpPost("save"), HttpPost, Authorize(Permissions.ContractPersonalDiscount), ProducesResponseType(typeof(ContractDiscount), 200)]
        [Event(EventCode.ContractDiscountSaved, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public IActionResult Save([FromBody] ContractDiscount contractDiscount)
        {
            ModelState.Validate();
            _contractDiscountService.Create(contractDiscount, _sessionContext.UserId);
            return Ok(contractDiscount);
        }

        [HttpPost("{id:int}/delete"), Authorize(Permissions.ContractDiscount)]
        [Event(EventCode.ContractDiscountDelete, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult Delete([FromRoute] int id)
        {
            _contractDiscountService.Delete(id);
            return Ok();
        }
    }
}