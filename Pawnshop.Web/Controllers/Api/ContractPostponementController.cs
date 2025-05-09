using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts.Postponements;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractView)]
    public class ContractPostponementController : Controller
    {
        private readonly ContractPostponementRepository _repository;
        private readonly ISessionContext _sessionContext;
        private readonly ContractRepository _contractRepository;

        public ContractPostponementController(ContractPostponementRepository repository, ISessionContext sessionContext,
            ContractRepository contractRepository)
        {
            _repository = repository;
            _sessionContext = sessionContext;
            _contractRepository = contractRepository;
        }

        [HttpPost]
        public ContractPostponement Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.ContractPostponement)]
        [Event(EventCode.ContractPostponementSave, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public ContractPostponement Save([FromBody] ContractPostponement model)
        {
            if (model == null)
            {
                throw new PawnshopApplicationException("Проверьте правильность ввода данных обязательных полей");
            }

            ModelState.Validate();

            using (var transaction = _repository.BeginTransaction())
            {
                if (model.Id > 0)
                {
                    _repository.Update(model);
                }
                else
                {
                    model.CreateDate = DateTime.Now;
                    model.AuthorId = _sessionContext.UserId;
                    _repository.Insert(model);
                }                        

                transaction.Commit();
            }

            return model;
        }

        [HttpPost, Authorize(Permissions.ContractPostponement)]
        [Event(EventCode.ContractPostponementDelete, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}