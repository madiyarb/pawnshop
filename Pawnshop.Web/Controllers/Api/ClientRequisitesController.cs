using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.Views;
using Pawnshop.Data.Models.JetPay;
using Pawnshop.Services.ClientRequisites;
using Pawnshop.Web.Extensions;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ClientRequisites.Bindings;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientRequisitesController : Controller
    {
        private readonly ISessionContext _sessionContext;
        public ClientRequisitesController(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        [HttpGet("clients/{id}")]
        [ProducesResponseType(typeof(ClientRequisiteListView), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetApplicationOnline(
            [FromRoute] int id,
            [FromServices] ClientRequisitesRepository repository)
        {

            return Ok(repository.GetClientRequisites(id));
        }

        [Authorize(Permissions.TasOnlineVerificator)]
        [HttpPost("clients/{clientid}/card")]
        [ProducesResponseType(typeof(ClientRequisiteCardView), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddCardClientRequisite(
            [FromRoute] int clientid,
            [FromServices] ClientRequisitesRepository repository,
            [FromBody] ClientRequisitesCardBinding binding)
        {
            int authorId = _sessionContext.UserId;

            var oldRequisites = repository.GetByValue(binding.CardNumber);

            if (!ClientRequisitesValidator.ValidateCard(binding.CardNumber))
            {
                return BadRequest("Неверный номер карты не соответствует алгоритму Луна");
            }
            if (oldRequisites != null)
            {
                return BadRequest($"Данные реквизиты принадлежат клиенту : {oldRequisites.ClientId} и не удалены. Идентификатор реквизитов {oldRequisites.Id} и не удалены ");
            }

            ClientRequisite clientRequisite = new ClientRequisite(clientid, binding.IsDefault, binding.CardNumber,
                binding.Note, authorId, binding.CardExpiryDate, binding.CardHolderName, ClientRequisiteCardType.Processing);
            if (!clientRequisite.IsCorrectValue())
            {
                return BadRequest($"Присланный номер карты {binding.CardNumber} не корректный.");
            }

            repository.SetAllOldClientRequisitesToNotDefault(clientid);
            repository.Insert(clientRequisite);
            return Ok(repository.GetClientRequisites(clientid).ClientRequisiteCards.FirstOrDefault(card => card.CardNumber == binding.CardNumber));
        }

        [Authorize(Permissions.TasOnlineVerificator)]
        [HttpPost("clients/{clientid}/bill")]
        [ProducesResponseType(typeof(ClientRequisiteBillView), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddBillClientRequisite(
            [FromRoute] int clientid,
            [FromServices] ClientRequisitesRepository repository,
            [FromBody] ClientRequisitesBillBinding binding)
        {
            int authorId = _sessionContext.UserId;

            var oldRequisites = repository.GetByValue(binding.IBAN);
            if (!ClientRequisitesValidator.ValidateIban(binding.IBAN))
            {
                return BadRequest("Неверный номер счета не прошел проверку согласно ISO 13616");
            }
            if (oldRequisites != null)
            {
                return BadRequest($"Данные реквизиты принадлежат клиенту : {oldRequisites.ClientId}. Идентификатор реквизитов {oldRequisites.Id} и не удалены");
            }

            ClientRequisite clientRequisite = new ClientRequisite(clientid, binding.BankId, binding.IsDefault,
                binding.IBAN, binding.Note, _sessionContext.UserId);
            if (!clientRequisite.IsCorrectValue())
            {
                return BadRequest($"Присланный номер счета {binding.IBAN} не корректный");
            }

            repository.SetAllOldClientRequisitesToNotDefault(clientid);
            repository.Insert(clientRequisite);
            return Ok(repository.GetClientRequisites(clientid).ClientRequisiteBills.FirstOrDefault(bill => bill.IBAN == binding.IBAN));
        }

        [Authorize(Permissions.TasOnlineVerificator)]
        [HttpPost("requisites/{requisiteid}/card")]
        [ProducesResponseType(typeof(ClientRequisiteCardView), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCardClientRequisite(
            [FromRoute] int requisiteid,
            [FromServices] ClientRequisitesRepository repository,
            [FromBody] ClientRequisitesCardBinding binding)
        {
            int authorId = _sessionContext.UserId;

            if (!ClientRequisitesValidator.ValidateCard(binding.CardNumber))
            {
                return BadRequest("Неверный номер карты не соответствует алгоритму Луна");
            }

            var oldRequisites = repository.GetByValue(binding.CardNumber);
            if (oldRequisites != null && oldRequisites.Id != requisiteid)
            {
                return BadRequest($"Данные реквизиты принадлежат клиенту : {oldRequisites.ClientId} и не удалены. Идентификатор реквизитов {oldRequisites.Id} и не удалены");
            }

            ClientRequisite clientRequisite = repository.Get(requisiteid);

            if (clientRequisite == null)
                return NotFound();

            clientRequisite.UpdateCard(binding.IsDefault, binding.CardNumber,
                binding.Note, authorId, binding.CardExpiryDate, binding.CardHolderName);

            if (!clientRequisite.IsCorrectValue())
            {
                return BadRequest($"Присланный номер карты {binding.CardNumber} не корректный.");
            }
            repository.SetAllOldClientRequisitesToNotDefault(clientRequisite.ClientId);
            repository.Update(clientRequisite);

            return Ok(repository.GetClientRequisites(clientRequisite.ClientId).ClientRequisiteCards.FirstOrDefault(card => card.CardNumber == binding.CardNumber));
        }

        [Authorize(Permissions.TasOnlineVerificator)]
        [HttpPost("requisites/{requisiteid}/bill")]
        [ProducesResponseType(typeof(ClientRequisiteCardView), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateBillClientRequisite(
            [FromRoute] int requisiteid,
            [FromServices] ClientRequisitesRepository repository,
            [FromBody] ClientRequisitesBillBinding binding)
        {
            int authorId = _sessionContext.UserId;

            var oldRequisites = repository.GetByValue(binding.IBAN);

            if (!ClientRequisitesValidator.ValidateIban(binding.IBAN))
            {
                return BadRequest("Неверный номер счета не прошел проверку согласно ISO 13616");
            }
            if (oldRequisites != null && oldRequisites.Id != requisiteid)
            {
                return BadRequest($"Данные реквизиты принадлежат клиенту : {oldRequisites.ClientId}. Идентификатор реквизитов {oldRequisites.Id} и не удалены");
            }

            ClientRequisite clientRequisite = repository.Get(requisiteid);
            if (clientRequisite == null)
                return NotFound();
            clientRequisite.UpdateBill(binding.BankId, binding.IsDefault, binding.IBAN, binding.Note, _sessionContext.UserId);
            if (!clientRequisite.IsCorrectValue())
            {
                return BadRequest($"Присланый номер счета {binding.IBAN} не корректный");
            }

            repository.SetAllOldClientRequisitesToNotDefault(clientRequisite.ClientId);
            repository.Update(clientRequisite);
            return Ok(repository.GetClientRequisites(clientRequisite.ClientId).ClientRequisiteBills.FirstOrDefault(bill => bill.IBAN == binding.IBAN));
        }

        [Authorize(Permissions.TasOnlineVerificator)]
        [HttpDelete("requisites/{requisiteid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRequisites(
            [FromRoute] int requisiteid,
            [FromServices] ClientRequisitesRepository repository)
        {
            var clientRequisites = repository.Get(requisiteid);
            if (clientRequisites == null)
            {
                return NotFound();
            }
            repository.Delete(requisiteid);
            return NoContent();
        }

        [HttpPost("applications/{applicationid}/requisites/create")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [AllowAnonymous]
        public async Task<IActionResult> CreateRequisitesForApplication(
            [FromRoute] Guid applicationid,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ClientRequisitesRepository clientRequisitesRepository,
            [FromServices] ApplicationOnlineFileRepository applicationOnlineFileRepository,
            [FromServices] ApplicationOnlineFileCodesRepository applicationOnlineFileCodesRepository,
            [FromServices] ClientRepository clientRepository,
            [FromBody] ClientRequisiteCreateBinding binding)
        {
            var application = applicationOnlineRepository.Get(applicationid);
            if (application == null)
                return NotFound();

            if (binding.Bill == null && binding.Card == null)
            {
                return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Request doesn't contains any requisite",
                    DRPPResponseStatusCode.BadData));
            }

            if (binding.Card != null)
            {
                if (!string.IsNullOrEmpty(binding.Card.CardNumber) && !string.IsNullOrEmpty(binding.Card.HolderName) && binding.Card.ExpireDate.HasValue)
                {
                    if (!ClientRequisitesValidator.ValidateCard(binding.Card.CardNumber))
                    {
                        return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Card number in request no pass LUNA check ",
                            DRPPResponseStatusCode.RequisiteNumberCheckFailed));
                    }
                    var oldRequisites = clientRequisitesRepository.GetByValue(binding.Card.CardNumber);
                    if (oldRequisites != null)
                    {
                        if (oldRequisites.ClientId == application.ClientId)
                        {
                            oldRequisites.IsDefault = true;
                            clientRequisitesRepository.Update(oldRequisites);
                            return NoContent();
                        }
                        return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Requisites ownned by client {oldRequisites.ClientId}, and not deleted {oldRequisites.Id}",
                            DRPPResponseStatusCode.RequisiteOwnedByOtherClient));
                    }

                    ClientRequisite clientRequisite = new ClientRequisite(application.ClientId, true, binding.Card.CardNumber, "", 1,
                        binding.Card.ExpireDate.Value.ToString("MM/yy", CultureInfo.InvariantCulture), binding.Card.HolderName, ClientRequisiteCardType.Processing);
                    if (!clientRequisite.IsCorrectValue())
                    {
                        return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Card incorrect ",
                            DRPPResponseStatusCode.RequisiteNumberCheckFailed));
                    }
                    clientRequisitesRepository.SetAllOldClientRequisitesToNotDefault(clientRequisite.ClientId);
                    clientRequisitesRepository.Insert(clientRequisite);
                }
                else
                {
                    var fileCode =
                        applicationOnlineFileCodesRepository.GetApplicationOnlineFileCodeByBusinessType("CREDIT_CARD_PHOTO");
                    Guid fileid = Guid.NewGuid();
                    await applicationOnlineFileRepository.Insert(new ApplicationOnlineFile(fileid, application.Id,
                        binding.Card.FileId, "image/jpeg", "image/jpeg", "",
                        () => this.UrlToAction<ApplicationOnlineFileController>(
                            nameof(ApplicationOnlineFileController.GetFile), new { fileid }),
                        fileCode.Id));
                    clientRequisitesRepository.SetAllOldClientRequisitesToNotDefault(application.ClientId);
                }
            }

            if (binding.Bill != null)
            {
                if (!ClientRequisitesValidator.ValidateIban(binding.Bill.Iban))
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"IBAN not pass  ISO 1361 check ",
                        DRPPResponseStatusCode.RequisiteNumberCheckFailed));
                }
                var bank = clientRepository.GetClientByBankCode(binding.Bill.BankCode);
                if (bank == null)
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Bank code not found in base",
                        DRPPResponseStatusCode.BankCodeNotFound));
                }

                var oldRequisites = clientRequisitesRepository.GetByValue(binding.Bill.Iban);
                if (oldRequisites != null)
                {
                    if (oldRequisites.ClientId == application.ClientId)
                    {
                        oldRequisites.IsDefault = true;
                        clientRequisitesRepository.Update(oldRequisites);
                        return NoContent();
                    }
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Requisites ownned by client {oldRequisites.ClientId}, and not deleted {oldRequisites.Id}",
                        DRPPResponseStatusCode.RequisiteOwnedByOtherClient));
                }

                ClientRequisite clientRequisite =
                    new ClientRequisite(application.ClientId, bank.Id, true, binding.Bill.Iban, "", 1);

                if (!clientRequisite.IsCorrectValue())
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"IBAN incorrect",
                        DRPPResponseStatusCode.RequisiteNumberCheckFailed));
                }
                clientRequisitesRepository.SetAllOldClientRequisitesToNotDefault(clientRequisite.ClientId);
                clientRequisitesRepository.Insert(clientRequisite);
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost("applications/{applicationid}/requisites/card/jetpay/create")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CreateRequisitesJetPayForApplication(
            [FromRoute] Guid applicationid,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ClientRequisitesRepository clientRequisitesRepository,
            [FromServices] JetPayCardPayoutInformationRepository jetPayCardPayoutInformationRepository,
            [FromBody] ClientJetPayRequisiteCreateBinding binding)
        {
            try
            {
                var application = applicationOnlineRepository.Get(applicationid);

                if (application == null)
                {
                    return NotFound();
                }

                var jetPayCardInfo = await jetPayCardPayoutInformationRepository.GetByTokenAsync(binding.Token);

                if (jetPayCardInfo != null && jetPayCardInfo.ClientRequisite.ClientId != application.ClientId)
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Requisites ownned by client {jetPayCardInfo.ClientRequisite.ClientId}, and not deleted {jetPayCardInfo.ClientRequisite.Id}",
                        DRPPResponseStatusCode.RequisiteOwnedByOtherClient));
                }

                if (jetPayCardInfo != null && jetPayCardInfo.Compare(binding.Token, binding.CustomerIp, binding.CustomerId, binding.MaskedCardNumber))
                {
                    return NoContent();
                }

                var clientRequisites = await clientRequisitesRepository.GetListByClientIdAsync(application.ClientId);

                var jetPayClientRequisites = clientRequisites.Where(x => x.CardType.HasValue && x.CardType.Value == ClientRequisiteCardType.JetPay);

                ClientRequisite clientRequisite = new ClientRequisite(application.ClientId, true, binding.MaskedCardNumber, string.Empty, 1,
                    binding.ExpireDate.Value.ToString("MM/yy", CultureInfo.InvariantCulture), binding.HolderName, ClientRequisiteCardType.JetPay);

                if (!clientRequisite.IsCorrectValue())
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Card incorrect ",
                        DRPPResponseStatusCode.RequisiteNumberCheckFailed));
                }

                using (var transaction = clientRequisitesRepository.BeginTransaction())
                {
                    try
                    {
                        foreach (var requisite in jetPayClientRequisites)
                        {
                            clientRequisitesRepository.Delete(requisite.Id);
                        }

                        clientRequisitesRepository.SetAllOldClientRequisitesToNotDefault(clientRequisite.ClientId);
                        clientRequisitesRepository.Insert(clientRequisite);

                        jetPayCardInfo = new JetPayCardPayoutInformation(clientRequisite.Id, binding.Token, binding.CustomerIp, binding.CustomerId);
                        await jetPayCardPayoutInformationRepository.InsertAsync(jetPayCardInfo);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode((int)HttpStatusCode.InternalServerError,
                            new BaseResponseDRPP(HttpStatusCode.InternalServerError, $"Error create record: {ex.Message}", DRPPResponseStatusCode.UnspecifiedDatabaseProblems));
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, $"Internal service error: {ex.Message}", DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }
    }
}
