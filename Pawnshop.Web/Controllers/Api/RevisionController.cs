using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Revisions;
using Pawnshop.Data.Models.Transfers;
using Pawnshop.Data.Models.Transfers.TransferContracts;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Security;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Models.Contract.Revision;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    public class RevisionController : Controller
    {
        private readonly RevisionRepository _repository;
        private readonly ContractRepository _contractRepository;

        public RevisionController(
            RevisionRepository repository, 
            ISessionContext sessionContext, 
            ContractRepository contractRepository)
        {
            _repository = repository;
            _contractRepository = contractRepository;
        }

        [HttpPost("/api/revision/upload")]
        public IActionResult Upload([FromBody] RevisionViewModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var contract = _contractRepository.Find(
                new
                {
                    model.ContractNumber
                });

            if (contract is null)
                throw new PawnshopApplicationException($"Контракт не с номером {model.ContractNumber} не найден");

            List<int> positionIds = _repository.GetPositionsByContractNumber(contract.Id, model.Date);

            if (!positionIds.Any())
                throw new PawnshopApplicationException("Позиции по данному номеру контракта не найдены");

            var resivions = new List<Revision>();

            using (var transaction = _repository.BeginTransaction())
            {
                positionIds.ForEach(contractId =>
                {
                    Revision contractRevision = new Revision()
                    {
                        PositionId = contractId,
                        RevisionDate = model.Date,
                        CreateDate = DateTime.Now,
                        Status = model.Status,
                        Note = model.Note,
                        ContractId = contract.Id
                    };

                    resivions.Add(contractRevision);

                    _repository.Insert(contractRevision);

                });
                transaction.Commit();
            }

            if (resivions.Any())
                return Ok(resivions);

            return NotFound("Ревизии не были добавлены");
        }

        [HttpPost("/api/revision/list")]
        public ListModel<Revision> List([FromBody] ListQueryModel<RevisionListQueryModel> listQuery)
        {
            listQuery ??= new ListQueryModel<RevisionListQueryModel>();
            listQuery.Model ??= new RevisionListQueryModel();

            return new ListModel<Revision>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }
    }
}