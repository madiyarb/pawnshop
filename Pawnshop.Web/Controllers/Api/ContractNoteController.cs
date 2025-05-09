using System;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.Contract;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractView)]
    public class ContractNoteController : Controller
    {
        private readonly ContractNoteRepository _repository;
        private readonly ISessionContext _sessionContext;

        public ContractNoteController(ContractNoteRepository repository,
            ISessionContext sessionContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<ContractNote> List([FromBody] ListQueryModel<ContractNoteListQueryModel> listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            if (listQuery.Model == null) throw new ArgumentNullException(nameof(listQuery.Model));

            return new ListModel<ContractNote>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public ContractNote Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _repository.Get(id);
            if (contract == null) throw new InvalidOperationException();

            return contract;
        }

        [HttpPost, Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractNoteSaved, EventMode = EventMode.Response, EntityType = EntityType.Contract)]
        public ContractNote Save([FromBody] ContractNote contractNote)
        {
            if (contractNote.Id == 0)
            {
                contractNote.NoteDate = DateTime.Now;
                contractNote.AuthorId = _sessionContext.UserId;
            }

            ModelState.Clear();
            TryValidateModel(contractNote);
            ModelState.Validate();

            if (contractNote.Id > 0)
            {
                _repository.Update(contractNote);
            }
            else
            {
                _repository.Insert(contractNote);
            }

            return contractNote;
        }

        [HttpPost, Authorize(Permissions.ContractManage)]
        [Event(EventCode.ContractNoteDeleted, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);

            return Ok();
        }
    }
}