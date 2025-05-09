using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.SUSNStatuses;
using Pawnshop.Services.SUSNStatuses;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.SUSNStatuses;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SUSNStatusesContoller : ControllerBase
    {
        private readonly SUSNStatusesRepository _repository;
        public SUSNStatusesContoller(SUSNStatusesRepository repository)
        {
            _repository = repository;
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SUSNStatus), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSusnStatus(
            [FromRoute] int id)
        {
            var status = await _repository.Get(id);

            if (status == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                    $"Статус сусн с идентификатором {id} не найден"));
            }
            return Ok(status);
        }

        [HttpPost("create")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateSusnStatus([FromBody] SUSNStatusBinding binding)
        {
            try
            {
                await _repository.Insert(new SUSNStatus(binding.Name, binding.NameKz, binding.Code, binding.Permanent, binding.Decline));

                return NoContent();
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponse(HttpStatusCode.InternalServerError, exception.Message));
            }
        }
        [HttpPut("{id}/update")]
        [ProducesResponseType(typeof(SUSNStatus), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateStatusSusn(
            [FromRoute] int id, 
            [FromBody] SUSNStatusBinding binding)
        {
            try
            {
                var status = await _repository.Get(id);
                if (status == null)
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                        $"Статус сусн с идентификатором {id} не найден"));
                status.Update(binding.Name, binding.NameKz, binding.Code, binding.Permanent, binding.Decline);
                await _repository.Update(status);
                return Ok(status);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponse(HttpStatusCode.InternalServerError, exception.Message));
            }
        }

        [HttpDelete("{id}/delete")]
        [ProducesResponseType(typeof(SUSNStatus), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeleteStatusSusn([FromRoute] int id)
        {
            try
            {
                var status = await _repository.Get(id);
                if (status == null)
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                        $"Статус сусн с идентификатором {id} не найден"));
                status.Delete();
                await _repository.Update(status);
                return Ok(status);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponse(HttpStatusCode.InternalServerError, exception.Message));
            }
        }


        [HttpGet("list")]
        [ProducesResponseType(typeof(SUSNStatus), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSusnStatuses([FromQuery] PageBinding binding)
        {
            var status = await _repository.GetListView(binding.Offset, binding.Limit);

            if (status == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                    $"Нету ни одного статуса сусн в бд"));
            }
            return Ok(status);
        }
    }


}
