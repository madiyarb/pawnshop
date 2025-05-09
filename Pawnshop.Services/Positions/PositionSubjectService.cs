using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Positions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Positions
{
    public class PositionSubjectService : IPositionSubjectService
    {
        private readonly PositionSubjectsRepository _positionSubjectsRepository;
        private readonly ISessionContext _sessionContext;
        private readonly PositionSubjectHistoryRepository _positionSubjectHistoryRepository;
        private readonly PositionRepository _positionRepository;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly ClientRepository _clientRepository;

        public PositionSubjectService(PositionSubjectsRepository positionSubjectsRepository, ISessionContext sessionContext,
            PositionSubjectHistoryRepository positionSubjectHistoryRepository, PositionRepository positionRepository,
            LoanSubjectRepository loanSubjectRepository, ClientRepository clientRepository)
        {
            _positionSubjectsRepository = positionSubjectsRepository;
            _sessionContext = sessionContext;
            _positionSubjectHistoryRepository = positionSubjectHistoryRepository;
            _positionRepository = positionRepository;
            _loanSubjectRepository = loanSubjectRepository;
            _clientRepository = clientRepository;
        }

        public PositionSubject Save(PositionSubject positionSubject)
        {
            if(positionSubject.Id > 0)
            {
                 _positionSubjectsRepository.Update(positionSubject);
            }
            else
            {
                _positionSubjectsRepository.Insert(positionSubject);
            }
            return positionSubject;
        }

        public PositionSubject Get(int id)
        {
            return _positionSubjectsRepository.Get(id);
        }

        public List<PositionSubject> GetSubjectsForPosition(int positionId)
        {

            return _positionSubjectsRepository.List(new ListQuery(), new { PositionId = positionId });
        }

        public List<PositionSubject> List(ListQuery query)
        {
            return _positionSubjectsRepository.List(query);
        }

        public List<PositionSubject> SaveSubjectsForPosition(List<PositionSubject> positionSubjects, int positionId)
        {
            if (positionSubjects != null)
            {
                foreach (var subject in positionSubjects)
                {
                    if(subject.Id == 0)
                    {
                        if (subject.PositionId == 0 && positionId == 0)
                            throw new PawnshopApplicationException("PositionId for PositionSubject was not set. Look at PositionSubjectService");

                        var authorId = Constants.ADMINISTRATOR_IDENTITY;
                        if (_sessionContext.IsInitialized)
                            authorId = _sessionContext.UserId;
                        subject.AuthorId = authorId;
                        subject.PositionId = positionId;
                        subject.CreateDate = DateTime.Now;
                        _positionSubjectsRepository.Insert(subject);
                    }
                }
            }

            DeleteRemovedSubjects(positionSubjects, positionId);

            return positionSubjects;
        }

        //удаление субъектов для позиции, если ранее были сохранены
        private void DeleteRemovedSubjects(List<PositionSubject> positionSubjects, int positionId)
        {
            var existingSubjects = new List<PositionSubject>();

            if (positionId > 0)
                existingSubjects.AddRange(_positionSubjectsRepository.GetOnlyPositionSubjectForPosition(positionId));

            var subjectIdsForDelete = new List<int>();

            if (existingSubjects.Any())
            {
                existingSubjects.ForEach(subject =>
                {
                    if (!positionSubjects.Exists(x => x.ClientId == subject.ClientId && x.SubjectId == subject.SubjectId && x.PositionId == subject.PositionId))
                        subjectIdsForDelete.Add(subject.Id);
                });
            }

            if (subjectIdsForDelete.Any())
            {
                foreach (var subjectId in subjectIdsForDelete)
                    _positionSubjectsRepository.Delete(subjectId);
            }
        }

        public async Task<List<PositionSubject>> GetPositionSubjectsForPositionAndDate(int positionId, DateTime? beginDate = null)
        {
            var mainPledger = await _positionSubjectHistoryRepository.FindMainPledgerForPosition(positionId, beginDate);
            var result = new List<PositionSubject>();


            if (mainPledger != null)
            {
                result.Add(mainPledger);
                var coPledgers = await _positionSubjectHistoryRepository.ListCoPledgersFromHistory(positionId, mainPledger.BeginDate);
                result.AddRange(coPledgers);
            }

            if(!result.Any())
            {
                var mainPledgerFromPosition = await _positionRepository.GetActivePositionClient(positionId);

                if(mainPledgerFromPosition != null)
                {
                    result.AddRange(GetSubjectsForPosition(positionId));
                    result.Add(await ConvertMainPledgerClientToPositionSubject(mainPledgerFromPosition, positionId));

                }

                await MigratePositionSubjectsToHistoryIfNecessary(positionId);

            }


            return result;
        }

        public async Task MigratePositionSubjectsToHistoryIfNecessary(int positionId)
        {

            var positionSubjects = new List<PositionSubject>();

            var collateralType = _positionRepository.Get(positionId)?.CollateralType;

            if (!collateralType.HasValue || collateralType != Pawnshop.AccountingCore.Models.CollateralType.Realty)
                return;

            var mainPledgerFromHistory = await _positionSubjectHistoryRepository.FindMainPledgerForPosition(positionId);

            if (mainPledgerFromHistory != null)
                return;

            var mainPledgerFromPosition = await _positionRepository.GetActivePositionClient(positionId);

            if (mainPledgerFromPosition != null)
            {
                positionSubjects.AddRange(GetSubjectsForPosition(positionId));
                positionSubjects.Add(await ConvertMainPledgerClientToPositionSubject(mainPledgerFromPosition, positionId));

            }
            if (positionSubjects.Any())
            {
                var signAndContractDate = await _positionRepository.GetSignAndContractDateForSignedPosition(positionId);
                var beginDate = signAndContractDate.Item1 ?? signAndContractDate.Item2;
                if (beginDate != null)
                {
                    var userId = Constants.ADMINISTRATOR_IDENTITY;
                    if(_sessionContext.IsInitialized)
                        userId = _sessionContext.UserId;

                    foreach (var positionSubject in positionSubjects)
                    {
                        var historyItem = new PositionSubjectHistory(positionSubject.PositionId, positionSubject.SubjectId, positionSubject.ClientId, beginDate.Value, userId);
                        _positionSubjectHistoryRepository.Insert(historyItem);
                    }
                }
            }
        }

        private async Task<PositionSubject> ConvertMainPledgerClientToPositionSubject(Client client, int positionId)
        {
            var result = new PositionSubject();
            var subject = _loanSubjectRepository.GetByCode(Constants.MAIN_PLEDGER_CODE);
            if (subject == null)
                throw new PawnshopApplicationException($"Could not find LoanSubject with code {Constants.MAIN_PLEDGER_CODE}");

            result.Subject = subject;
            result.SubjectId = subject.Id;
            result.Client = client;
            result.ClientId = client.Id;
            result.AuthorId = client.AuthorId;
            result.PositionId = positionId;
            result.CreateDate = DateTime.Now;


            return result;
        }

        public async Task SavePositionSubjectsToHistoryForContract(Contract contract)
        {
            if (contract.CollateralType != Pawnshop.AccountingCore.Models.CollateralType.Realty)
                return;

            foreach(var position in contract.Positions)
            {
                if (position.Position.CollateralType != Pawnshop.AccountingCore.Models.CollateralType.Realty)
                    continue;

                var mainPledger = position.Position.Client;
                var coPledgers = position.Position.PositionSubjects.Where(x => x.Subject.Code == Constants.PLEDGER_CODE);

                if (await AreAllPledgersInHistoryAlready(position))
                {
                    continue;
                }

                if (mainPledger == null)
                    throw new PawnshopApplicationException("Не указан клиент для позиции договора - Client not filled in PositionSubjectService - SavePositionSubjectsToHistoryForContract");

                var subjectMainPledger = _loanSubjectRepository.GetByCode(Constants.MAIN_PLEDGER_CODE);

                var authorId = Constants.ADMINISTRATOR_IDENTITY;
                if (_sessionContext.IsInitialized)
                    authorId = _sessionContext.UserId;

                var mainPledgerAsHistoryItem = new PositionSubjectHistory(position.PositionId, subjectMainPledger.Id, mainPledger.Id, contract.SignDate ?? contract.ContractDate, authorId);
                _positionSubjectHistoryRepository.Insert(mainPledgerAsHistoryItem);

                var subjectCoPledger = _loanSubjectRepository.GetByCode(Constants.PLEDGER_CODE);

                foreach (var coPledger in coPledgers)
                {
                    var coPledgerAsHistoryItem = new PositionSubjectHistory(position.PositionId, subjectCoPledger.Id, coPledger.ClientId, mainPledgerAsHistoryItem.BeginDate, authorId);
                    _positionSubjectHistoryRepository.Insert(coPledgerAsHistoryItem);
                }

            }
        }

        private async Task<bool> AreAllPledgersInHistoryAlready(ContractPosition contractPosition)
        {

            var mainPledger = contractPosition.Position.Client;
            var coPledgers = contractPosition.Position.PositionSubjects.Where(x => x.Subject.Code == Constants.PLEDGER_CODE);
            var result = true;

            var mainPledgerInHistory = await _positionSubjectHistoryRepository.FindMainPledgerForPosition(contractPosition.PositionId);
            if (mainPledgerInHistory == null)
                return false;

            if (mainPledgerInHistory.ClientId != mainPledger.Id)
                return false;

            var coPledgersInHistory = await _positionSubjectHistoryRepository.ListCoPledgersFromHistory(contractPosition.PositionId, mainPledgerInHistory.BeginDate);

            if (coPledgers.Count() != coPledgersInHistory.Count())
                return false;


            foreach(var coPledger in coPledgers)
            {
                if (!coPledgersInHistory.Any(x => x.ClientId == coPledger.ClientId && x.PositionId == coPledger.PositionId && x.SubjectId == coPledger.SubjectId))
                    return false;
            }

            return result;
        }
    }
}
