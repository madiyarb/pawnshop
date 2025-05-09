using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine;
using System;
using Pawnshop.Data.Access;
using Pawnshop.Web.Models.Clients;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Engine.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ClientEstimationCompanyView)]
    public class EstimationCompanyController : Controller
    {
        private readonly ClientRepository _repository;
        private readonly IDomainService _domainService;
        private readonly ISessionContext _sessionContext;
        public EstimationCompanyController(ClientRepository repository, IDomainService domainService, ISessionContext sessionContext)
        {
            _repository = repository;
            _domainService = domainService;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/EstimationCompany/list")]
        public ListModel<Client> List([FromBody] ListQueryModel<ClientListQueryModel> listQuery)
        {
            var subTypeId = _domainService.GetDomainValue(Constants.CLIENT_SUB_TYPE, Constants.ESTIMATION_COMPANY).Id;
            return new ListModel<Client>
            {
                List = _repository.ListEstimationCompanies(listQuery, subTypeId),
                Count = _repository.CountEstimationCompanies(listQuery, subTypeId)
            };
        }

        [HttpPost("/api/EstimationCompany/save"), Authorize(Permissions.ClientEstimationCompanyManage)]
        [Event(EventCode.ClientSaved, EventMode = EventMode.Response, EntityType = EntityType.Client)]
        public IActionResult Save([FromBody] Client model)
        {
            ModelState.Validate();

            var validationErrors = new List<string>();
            if (string.IsNullOrEmpty(model.IdentityNumber))
                validationErrors.Add("БИН не может быть пустым");
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.FullName))
                validationErrors.Add("Наименование оценочной компаний(каз/рус) не может быть пустым");
            if (string.IsNullOrEmpty(model.DocumentNumber))
                validationErrors.Add("Номер лицензии не может быть пустым");
            if (!model.DocumentDate.HasValue)
                validationErrors.Add("Дата лицензии не может быть пустым");
            if (model.LegalFormId == 0)
                validationErrors.Add("Выберите правовую форму");

            if (validationErrors.Count > 1)
                throw new PawnshopApplicationException(validationErrors.ToArray());

            try
            {
                if (model.Id > 0)
                {
                    _repository.Update(model);
                }
                else
                {
                    var client = _repository.FindByIdentityNumber(model.IdentityNumber);
                    if (client != null)
                        throw new Exception("Невозможно создать клиента, клиент уже существут");

                    model.AuthorId = _sessionContext.UserId;
                    model.CreateDate = DateTime.Now;
                    model.CardType = CardType.Standard;
                    model.SubTypeId = _domainService.GetDomainValue(Constants.CLIENT_SUB_TYPE, Constants.ESTIMATION_COMPANY).Id;
                    _repository.Insert(model);
                }
            }
            catch (SqlException e)
            {
                throw new PawnshopApplicationException(e.Message);
            }
            catch (PawnshopApplicationException e)
            {
                var message = string.Empty;
                message = e.Message.Replace("REQUISITE_MUST_BE_UNIQUE", "Реквизит должен быть уникальным");
                message = e.Message.Replace("DOCUMENT_MUST_BE_UNIQUE", "Документ должен быть уникальным");
                message = e.Message.Replace("ADDRESS_MUST_BE_UNIQUE", "Адрес должен быть уникальным");

                throw new PawnshopApplicationException(string.IsNullOrEmpty(message) ? e.Message : message);
            }

            return Ok(model);
        }
        [HttpPost("/api/EstimationCompany/delete"), Authorize(Permissions.ClientEstimationCompanyManage)]
        [Event(EventCode.ClientDeleted, EventMode = EventMode.Request, EntityType = EntityType.Client)]
        public IActionResult Delete([FromBody] int id)
        {
            ModelState.Validate();

            var count = _repository.RelationCountEstCompany(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить клиента, так как он привязан к позиций");
            }

            _repository.Delete(id);
            return Ok();
        }

        [HttpPost("/api/EstimationCompany/card"), ProducesResponseType(typeof(Client), 200)]
        public IActionResult Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var result = _repository.Get(id);

            if (result == null) throw new InvalidOperationException();
            return Ok(result);
        }
    }
}
