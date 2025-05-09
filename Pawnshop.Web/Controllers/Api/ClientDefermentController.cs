using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Web.Models.List;
using System.Threading.Tasks;
using Pawnshop.Data.Models.ClientDeferments;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Data.Access;
using System.Linq;
using Pawnshop.Data.Models.TasLabRecruit;
using Pawnshop.Services.TasLabRecruit;
using System.Collections.Generic;
using System;
using Pawnshop.Services.Domains;
using Pawnshop.Core.Extensions;

namespace Pawnshop.Web.Controllers.Api
{
    public class ClientDefermentController : Controller
    {
        private readonly ClientDefermentRepository _clientDefermentRepository;
        private readonly IDomainService _domainService;
        private readonly ClientRepository _clientRepository;
        private readonly ITasLabRecruitService _recruitService;
        private readonly IClientDefermentService _defermentService;

        public ClientDefermentController(
            ClientDefermentRepository clientDefermentRepository,
            IDomainService domainService,
            ClientRepository clientRepository,
            ITasLabRecruitService recruitService,
            IClientDefermentService defermentService)
        {
            _clientDefermentRepository = clientDefermentRepository;
            _domainService = domainService;
            _clientRepository = clientRepository;
            _recruitService = recruitService;
            _defermentService = defermentService;
        }

        [Authorize(Permissions.RestructuringPageView)]
        [HttpPost("/api/clientDeferments/list")]
        public async Task<IActionResult> List([FromBody] ListQueryModel<ClientDefermentsListQueryModel> listQuery)
        {
            var list = _clientDefermentRepository.Find(listQuery, listQuery.Model)
                .Select(deferment => _defermentService.MapDefermentToModel(deferment))
                .ToList();

            var count = _clientDefermentRepository.Count(listQuery, listQuery.Model);

            return Ok(new ListModel<ClientDefermentModel> { List = list, Count = count });
        }

        [Authorize(Permissions.RestructuringRecruitManage)]
        [HttpPost("/api/clientDeferments/recruitByIIN")]
        public async Task<IActionResult> GetRecruitByIIN([FromBody] RecruitByIINRequestModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.IIN))
            {
                return BadRequest(new { Message = "Invalid request data" });
            }
            var recruitIINResponse = await _recruitService.GetRecruitByIIN(request.IIN);
            await _defermentService.RegisterRecruitDeferment(new Recruit
            {
                IIN = recruitIINResponse.IIN,
                Status = recruitIINResponse.Status,
                Date = recruitIINResponse.Date,
            });

            var client = await _clientRepository.GetByIdentityNumberAsync(request.IIN);
            var deferments = _clientDefermentRepository.Find(null, new { ClientId = client.Id });
            if (deferments.Count() == 0)
                return Ok(new { Message = "У клиента нет активных контратов!" });

            var result = deferments.Select(deferment => _defermentService.MapDefermentToModel(deferment, client)).ToList();

            return Ok(result);
        }


        [HttpPost("/api/clientDeferments/activeDeferments")]
        public IActionResult FindActiveDeferments([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var deferments = _defermentService.GetActiveDeferments(id);
            if (!deferments.Any())
                return Ok();

            var message = new List<string>();
            foreach(var deferment in deferments)
            {
                message.Add($"Клиент иммет статус: {_domainService.GetDomainValue(Constants.DEFERMENT_TYPES_TYPE_DOMAIN, deferment.DefermentTypeId).Name}");
                message.Add($"Дата начала отсрочки: {deferment?.StartDate.ToString("dd.MM.yyyy")}");
                message.Add($"Дата окончания отсрочки: {deferment?.EndDate.ToString("dd.MM.yyyy")}");
                message.Add($"Контракт: {deferment.ContractId}");
            }
            return Ok(message);
        }
    }
}
