using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.BranchesPartnerCodes;
using Pawnshop.Data.Models.BranchesPartnerCodes.Query;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.BranchesPartnerCodes;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class BranchesPartnerCodesController : Controller
    {
        private readonly BranchesPartnerCodesRepository _repository;
        public BranchesPartnerCodesController(BranchesPartnerCodesRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("BranchesPartnerCodes/create")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] BranchPartnerCodeCreationBinding binding)
        {
            Guid id = Guid.NewGuid();
            if (binding.Id != null)
            {
                id = binding.Id.Value;
            }

            await _repository.Insert(new BranchesPartnerCode(id, binding.BranchId, binding.PartnerCode));
            return Ok();
        }

        [HttpPost("BranchesPartnerCode/{id}/update")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update([FromRoute] Guid id,
            [FromBody] BranchPartnerCodeUpdateBinding binding)
        {
            var branchesPartnerCode = await _repository.Get(id);
            if (branchesPartnerCode == null)
                return NotFound();
            branchesPartnerCode.Update(binding.BranchId, binding.PartnerCode, binding.DeleteDate, binding.Enabled);
            await _repository.Update(branchesPartnerCode);
            return Ok(branchesPartnerCode);
        }


        [HttpPost("BranchesPartnerCodes/{id}/disable")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Disable([FromRoute] Guid id)
        {
            var code = await _repository.Get(id);
            if (code == null)
                return NotFound();
            code.Disable();
            await _repository.Update(code);
            return Ok();
        }

        [HttpPost("BranchesPartnerCodes/{id}/enable")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Enable([FromRoute] Guid id)
        {
            var code = await _repository.Get(id);
            if (code == null)
                return NotFound();
            code.Enable();
            await _repository.Update(code);
            return Ok();
        }


        [HttpDelete("BranchesPartnerCodes/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var code = await _repository.Get(id);
            if (code == null)
                return NotFound();
            code.Delete();
            await _repository.Update(code);
            return Ok();
        }

        [HttpGet("BranchesPartnerCodes/list")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetList(
            [FromQuery] BranchPartnerCodeListQuery query,
            [FromQuery] PageBinding pageBinding )
        {
            return Ok(await _repository.GetListView(query, pageBinding.Offset, pageBinding.Limit));
        }

        [HttpGet("BranchesPartnerCodes/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(
            [FromRoute] Guid id)
        {
            var branchPartnerCode = await _repository.Get(id);
            if (branchPartnerCode == null)
                return NotFound();
            return Ok(branchPartnerCode);
        }

    }
}
