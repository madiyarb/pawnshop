using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Leads;
using Pawnshop.Data.Models.OnlineTasks.Events;
using Pawnshop.Data.Models.OnlineTasks.Views;
using Pawnshop.Data.Models.OnlineTasks;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Hubs;
using Pawnshop.Web.Models.OnlineTask;
using Pawnshop.Web.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using Pawnshop.Services.TasOnlinePermissionValidator;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnlineTasksController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private static JsonSerializerOptions _serializerOptions;
        private readonly BranchContext _branchContext;
        private readonly ITasOnlinePermissionValidatorService _permissionValidator;

        public OnlineTasksController(
            ISessionContext sessionContext,
            BranchContext branchContext,
            ITasOnlinePermissionValidatorService permissionValidator)
        {
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _permissionValidator = permissionValidator;
        }

        [HttpGet("onlinetasks/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetView(
            [FromRoute] Guid id,
            [FromServices] OnlineTasksRepository repository)
        {
            var task = repository.GetView(id);
            if (task == null)
                return NotFound();
            task.FillStatuses();
            return Ok(task);
        }

        [Authorize(Permissions.TasOnlineManager)]
        [HttpPost("onlinetasks/create")]
        [ProducesResponseType(typeof(OnlineTask), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOnlineTask(
            [FromBody] OnlineTaskCreationBinding binding,
            [FromServices] OnlineTasksRepository repository,
            [FromServices] IHubContext<TasOnlineUsersHub> userHab,
            CancellationToken cancellationToken)
        {
            OnlineTaskType typeEnum;
            if (!Enum.TryParse(binding.Type, out typeEnum))
            {
                return BadRequest($"Не найден присланный тип задачи {binding.Type}");
            }

            var task = new OnlineTask(binding.Id, binding.Type, _sessionContext.UserId, binding.Decription,
                binding.ShortDescription, binding.UserId, binding.ClientId, binding.ApplicationId);
            repository.Insert(task);
            await userHab.Clients.Group($"Users")
                .SendAsync("TaskCreated", JsonSerializer.Serialize(new OnlineTaskCreated
                {
                    ApplicationId = task.ApplicationId,
                    ClientId = task.ClientId,
                    CompleteDate = task.CompleteDate,
                    CreateDate = task.CreateDate,
                    CreationUserId = task.CreationUserId,
                    Description = task.Description,
                    Done = task.Done,
                    Id = task.Id,
                    ShortDescription = task.ShortDescription,
                    Status = task.Status,
                    UserId = task.UserId
                }, _serializerOptions), cancellationToken);
            return Ok(task);
        }

        [HttpPost("onlinetasks/create/recall")]
        [ProducesResponseType(typeof(OnlineTask), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateRecallTask(
            [FromBody] OnlineTaskCreationBinding binding,
            [FromServices] OnlineTasksRepository repository,
            [FromServices] IHubContext<TasOnlineUsersHub> userHab,
            [FromServices] LeadsRepository leadsRepository,
            CancellationToken cancellationToken)
        {
            var lead = new Lead(binding.Lead.Id, binding.Lead.Name, binding.Lead.Surname, binding.Lead.Patronymic,
                binding.Lead.Phone);

            await leadsRepository.Insert(lead);

            var task = new OnlineTask(binding.Id, OnlineTaskType.CallBack.ToString(), _sessionContext.UserId,
                $"Пользователь заказал обратный звонок на номер {binding.Lead.Phone}",
                $"Обратный звонок на номер {binding.Lead.Phone}", lead.Id);
            repository.Insert(task);
            await userHab.Clients.Group($"Users")
                .SendAsync("TaskCreated", JsonSerializer.Serialize(new OnlineTaskCreated
                {
                    ApplicationId = task.ApplicationId,
                    ClientId = task.ClientId,
                    CompleteDate = task.CompleteDate,
                    CreateDate = task.CreateDate,
                    CreationUserId = task.CreationUserId,
                    Description = task.Description,
                    Done = task.Done,
                    Id = task.Id,
                    ShortDescription = task.ShortDescription,
                    Status = task.Status,
                    UserId = task.UserId
                }, _serializerOptions), cancellationToken);
            return Ok(task);
        }

        [Authorize(Permissions.TasOnlineManager)]
        [HttpPost("onlinetasks/{id}/processing")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Processing(
            [FromRoute] Guid id,
            [FromServices] OnlineTasksRepository repository)
        {
            var task = repository.Get(id);
            if (task == null)
                return NotFound();
            task.Processing(_sessionContext.UserId);
            repository.Update(task);
            return NoContent();
        }

        [HttpPost("onlinetasks/{id}/complete")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Complete(
            [FromRoute] Guid id,
            [FromServices] OnlineTasksRepository repository)
        {
            if (!_permissionValidator.AnyRole())
                return StatusCode(403); 
            var task = repository.Get(id);
            if (task == null)
                return NotFound();
            task.Complete(_sessionContext?.UserId);
            repository.Update(task);
            return NoContent();
        }

        [HttpGet("onlinetasks/list")]
        [ProducesResponseType(typeof(OnlineTaskListView), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetList(
            CancellationToken cancellationToken,
            [FromQuery] PageBinding pageBinding,
            [FromQuery] OnlineTaskListFilterBinding binding,
            [FromServices] OnlineTasksRepository repository)
        {
            DateTime startDate = DateTime.UnixEpoch;
            DateTime endDate = DateTime.MaxValue.AddYears(-1);
            if (binding.StartDate != null)
            {
                startDate = binding.StartDate.Value;
            }

            if (binding.EndDate != null)
            {
                endDate = binding.EndDate.Value;
            }
            var list = await repository.GetListView(pageBinding.Offset, pageBinding.Limit, startDate, endDate, _sessionContext?.UserId, binding.PartnerCode, binding.MinutesLeftAfterUpdate, _branchContext.Branch.Id);
            if (list == null)
            {
                return NotFound();
            }
            return Ok(list);
        }

        [HttpGet("onlinetaskStatuses/list")]
        [ProducesResponseType(typeof(List<EnumView>), 200)]
        public async Task<IActionResult> GetOnlinetaskStatuses()
        {
            List<EnumView> onlineTaskStatuses = new List<EnumView>();

            foreach (OnlineTaskStatus status in Enum.GetValues(typeof(OnlineTaskStatus)))
            {
                onlineTaskStatuses.Add(new EnumView
                {
                    Id = (int)status,
                    Name = status.ToString(),
                    DisplayName = status.GetDisplayName()
                });
            }

            return Ok(onlineTaskStatuses);
        }

        [HttpGet("onlinetaskTypes/list")]
        [ProducesResponseType(typeof(List<EnumView>), 200)]
        public async Task<IActionResult> GetOnlinetaskTypes()
        {
            List<EnumView> onlineTaskTypes = new List<EnumView>();

            foreach (OnlineTaskType status in Enum.GetValues(typeof(OnlineTaskType)))
            {
                onlineTaskTypes.Add(new EnumView
                {
                    Id = (int)status,
                    Name = status.ToString(),
                    DisplayName = status.GetDisplayName()
                });
            }

            return Ok(onlineTaskTypes);
        }
    }
}
