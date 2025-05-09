using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Pawnshop.Core;
using Pawnshop.Core.Utilities;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Calls;
using Pawnshop.Data.Models.Interaction;
using Pawnshop.Data.Models.Leads;
using Pawnshop.Data.Models.OnlineTasks;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Pbx;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("restapi/services/run")]
    [ApiController]
    public class PbxController : ControllerBase
    {
        private readonly CallsRepository _callsRepository;
        private readonly CallBlackListRepository _callBlackListRepository;
        private readonly UserRepository _userRepository;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly InteractionsRepository _interactionsRepository;
        private readonly ISignalRNotificationService _iSignalRNotificationService;
        private readonly ClientRepository _clientRepository;
        private readonly LeadsRepository _leadsRepository;

        public PbxController(
            CallsRepository callsRepository,
            CallBlackListRepository callBlackListRepository,
            UserRepository userRepository,
            ClientContactRepository clientContactRepository,
            InteractionsRepository interactionsRepository,
            ISignalRNotificationService signalRNotificationService,
            ClientRepository clientRepository,
            LeadsRepository leadsRepository)
        {
            _callsRepository = callsRepository;
            _callBlackListRepository = callBlackListRepository;
            _userRepository = userRepository;
            _clientContactRepository = clientContactRepository;
            _interactionsRepository = interactionsRepository;
            _iSignalRNotificationService = signalRNotificationService;
            _clientRepository = clientRepository;
            _leadsRepository = leadsRepository;
        }


        [HttpGet("crm_is_blacklisted")]
        public ActionResult<object> CheckBlackListFromPbx([FromQuery(Name = "num")] string phoneNumber)
        {
            var editPhoneNumber = RegexUtilities.GetNumbers(phoneNumber);

            if (!RegexUtilities.IsValidKazakhstanPhone(editPhoneNumber))
                return BadRequest($"Incorrect phone number {phoneNumber}");

            var result = _callBlackListRepository.List(null, new { PhoneNumber = phoneNumber });

            return Ok(result?.Any() ?? false);
        }

        [HttpGet("crm_incoming_call")]
        public async Task<IActionResult> IncomingFromPbx([FromQuery] IncomingRequest rq, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(rq.PhoneNumber) || string.IsNullOrEmpty(rq.CallPbxId) || string.IsNullOrEmpty(rq.UserInternalPhone))
                return BadRequest("Need fill all parameters.");

            var phoneNumber = RegexUtilities.GetNumbers(rq.PhoneNumber);

            if (!RegexUtilities.IsValidKazakhstanPhone(phoneNumber))
                return BadRequest($"Incorrect phone number {rq.PhoneNumber}.");

            var clientId = _clientContactRepository.GetClientIdByDefaultPhone(phoneNumber);
            string firstName = null;
            string surname = null;
            string patronymic = null;

            if (clientId != null)
            {
                var client = _clientRepository.GetOnlyClient(clientId.Value);
                firstName = client.Name;
                surname = client.Surname;
                patronymic = client.Patronymic;
            }

            var call = new Call
            {
                CallPbxId = rq.CallPbxId,
                Direction = "incoming",
                Language = rq.Language,
                PhoneNumber = phoneNumber,
                UserInternalPhone = rq.UserInternalPhone,
            };

            _callsRepository.Insert(call);

            var userId = _userRepository.GetByInternalPhone(rq.UserInternalPhone);
            Interaction interaction = null;

            if (userId.HasValue)
            {
                interaction = new Interaction(userId.Value,
                    InteractionType.CALL_INCOMING,
                    null,
                    phoneNumber,
                    "",
                    "",
                    firstName,
                    surname,
                    patronymic,
                    rq.Language,
                    callId: call.Id,
                    clientId: clientId);
            }
            else
            {
                interaction = new Interaction(Constants.ADMINISTRATOR_IDENTITY,
                    InteractionType.CALL_INCOMING,
                    null,
                    phoneNumber,
                    "",
                    "",
                    firstName,
                    surname,
                    patronymic,
                    rq.Language,
                    callId: call.Id,
                    clientId: clientId);
            }

            _interactionsRepository.Insert(interaction);

            if (userId.HasValue)
            {
                await _iSignalRNotificationService.NotifyUser(new InteractionCreated { InteractionId = interaction.Id, Interaction = interaction }, userId.Value,
                    cancellationToken);
            }
            else
            {
                await _iSignalRNotificationService.NotifyAllUsers(new InteractionCreated { InteractionId = interaction.Id, Interaction = interaction },
                    cancellationToken);
            }

            return Ok();
        }

        [HttpGet("crm_outgoing_call")]
        public async Task<IActionResult> OutgoingFromPbx([FromQuery] OutgoingRequest rq, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(rq.PhoneNumber) || string.IsNullOrEmpty(rq.CallPbxId) || string.IsNullOrEmpty(rq.UserInternalPhone))
                return BadRequest("Need fill all parameters.");

            var phoneNumber = RegexUtilities.GetNumbers(rq.PhoneNumber);

            if (!RegexUtilities.IsValidKazakhstanPhone(phoneNumber))
                return BadRequest($"Incorrect phone number {rq.PhoneNumber}.");

            var call = new Call
            {
                CallPbxId = rq.CallPbxId,
                Direction = "outgoing",
                PhoneNumber = phoneNumber,
                UserInternalPhone = rq.UserInternalPhone,
            };

            var clientId = _clientContactRepository.GetClientIdByDefaultPhone(phoneNumber);
            string firstName = null;
            string surname = null;
            string patronymic = null;

            if (clientId != null)
            {
                var client = _clientRepository.GetOnlyClient(clientId.Value);
                firstName = client.Name;
                surname = client.Surname;
                patronymic = client.Patronymic;
            }

            _callsRepository.Insert(call);

            var userId = _userRepository.GetByInternalPhone(rq.UserInternalPhone);

            var interaction = _interactionsRepository.FindForLink(userId);

            if (interaction == null)
            {
                if (userId.HasValue)
                {
                    interaction = new Interaction(userId.Value,
                        InteractionType.CALL_OUTGOING,
                        rq.UserInternalPhone,
                        phoneNumber,
                        "",
                        "",
                        firstName,
                        surname,
                        patronymic,
                        null,
                        callId: call.Id,
                        clientId: clientId);
                }
                else
                {
                    interaction = new Interaction(Constants.ADMINISTRATOR_IDENTITY,
                        InteractionType.CALL_OUTGOING,
                        rq.UserInternalPhone,
                        phoneNumber,
                        "",
                        "",
                        firstName,
                        surname,
                        patronymic,
                        null,
                        callId: call.Id,
                        clientId: clientId);
                }

                _interactionsRepository.Insert(interaction);
            }
            else
            {
                interaction.CallId = call.Id;

                _interactionsRepository.Update(interaction);
            }

            if (userId.HasValue)
            {
                await _iSignalRNotificationService.NotifyUser(new InteractionCreated { InteractionId = interaction.Id, Interaction = interaction }, userId.Value,
                    cancellationToken);
            }
            else
            {
                await _iSignalRNotificationService.NotifyAllUsers(new InteractionCreated { InteractionId = interaction.Id, Interaction = interaction },
                    cancellationToken);
            }

            return Ok();
        }

        [HttpGet("crm_status_call")]
        public async Task<IActionResult> SaveStatus([FromServices] OnlineTasksRepository onlineTasksRepository,
            [FromQuery] SaveStatusRequest rq)
        {
            if (string.IsNullOrEmpty(rq.CallPbxId))
                return BadRequest("Need fill all parameters.");

            var phoneNumber = RegexUtilities.GetNumbers(rq.PhoneNumber);

            var call = _callsRepository.GetByCallPbxId(rq.CallPbxId) ?? new Call();
            var userId = string.IsNullOrEmpty(rq.UserInternalPhone) ? null : _userRepository.GetByInternalPhone(rq.UserInternalPhone);
            var clientId = _clientContactRepository.GetClientIdByDefaultPhone(phoneNumber);

            call.CallPbxId = rq.CallPbxId;
            call.ClientId = clientId;
            call.CompanyInternalPhone = rq.CompanyInternalPhone;
            call.Direction = rq.Direction;
            call.Duration = rq.Duration;
            call.Language = rq.Language;
            call.PhoneNumber = phoneNumber;
            call.RecordFile = rq.RecordFile;
            call.Status = rq.Status;
            call.UserId = userId;
            call.UserInternalPhone = rq.UserInternalPhone;

            if (call.Id == 0)
                _callsRepository.Insert(call);
            else
                _callsRepository.Update(call);

            if (call.Direction == "incoming" && call.Status == "NOANSWER")
            {
                string name = "";
                string surname = "";
                string patronimic = "";
                if (clientId != null)
                {
                    var client = _clientRepository.GetOnlyClient(clientId.Value);
                    name = client.Name;
                    surname = client.Surname;
                    patronimic = client.Patronymic;
                }
                var lead = await _leadsRepository.GetByNumber(phoneNumber);

                if (lead == null)
                {
                    lead = new Lead(Guid.NewGuid(), name, surname, patronimic, phoneNumber);
                    await _leadsRepository.Insert(lead);
                }

                onlineTasksRepository.Insert(new OnlineTask(Guid.NewGuid(), OnlineTaskType.CallBack.ToString(),
                    Constants.ADMINISTRATOR_IDENTITY, $"Перезвоните клиенту по номеру {phoneNumber}",
                    "Перезвоните клиенту", userId, clientId, callId: call.Id, leadId: lead.Id));

            }

            if (call.Direction == "outgoing" && call.Status == "ANSWER")
            {
                var notCompletedTasks = onlineTasksRepository.GetAllCallbackNotCompletedTasksByMissingCalls(phoneNumber);
                notCompletedTasks.InsertRange(0, onlineTasksRepository.GetAllNotCompletedCallBackTasksByLead(phoneNumber));

                for (int i = 0; i < notCompletedTasks.Count; i++)
                {
                    notCompletedTasks[i].Complete();
                    onlineTasksRepository.Update(notCompletedTasks[i]);
                }
            }

            return Ok();
        }
    }
}
