using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.LegalCollection.Details;
using Pawnshop.Data.Models.LegalCollection.Details.HttpService;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Collection.http;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCasesDetailConverter : ILegalCasesDetailConverter
    {
        private readonly ICollectionHttpService<CollectionReason> _collectionService;
        private readonly ILegalCollectionRepository _legalCollectionRepository;
        private readonly IContractService _contractService;
        private readonly IInscriptionService _inscriptionService;
        private readonly UserRepository _users;

        public LegalCasesDetailConverter(
            ICollectionHttpService<CollectionReason> collectionService,
            ILegalCollectionRepository legalCollectionRepository,
            IContractService contractService,
            IInscriptionService inscriptionService,
            UserRepository users
            )
        {
            _collectionService = collectionService;
            _legalCollectionRepository = legalCollectionRepository;
            _contractService = contractService;
            _inscriptionService = inscriptionService;
            _users = users;
        }

        public async Task<List<LegalCasesDetailsViewModel>> ConvertAsync(List<LegalCaseDetailsResponse> detailsResponse)
        {
            var reasons = await _collectionService.List();

            var detailsLegalCase = new List<LegalCasesDetailsViewModel>();

            foreach (var legalCase in detailsResponse)
            {
                var userIds = legalCase.Histories
                    .Select(h => h.AuthorId)
                    .Where(userId => userId != null)
                    .Distinct()
                    .ToList();

                var users = userIds
                    .Select(userId => _users.Get((int)userId))
                    .Where(foundUser => foundUser != null)
                    .ToList();

                var reason = reasons.FirstOrDefault(r => r.Id == legalCase.CaseReasonId);
                var contractInfo = await _legalCollectionRepository.GetContractInfoAsync(legalCase.ContractId);
                var user = legalCase.AuthorId != null ? await _users.GetAsync((int)legalCase.AuthorId) : null;
                var contract = await _contractService.GetOnlyContractAsync(legalCase.ContractId);
                DateTimeOffset? accrualStopDate = null;
                decimal totalSum = 0;

                if (contract.InscriptionId.HasValue)
                {
                    var inscription = await _inscriptionService.GetAsync((int)contract.InscriptionId);
                    if (inscription != null)
                    {
                        accrualStopDate = new DateTimeOffset(inscription.Date).Date;
                        totalSum += inscription.TotalCost;
                    }
                }

                var detailsViewModel = new LegalCasesDetailsViewModel
                {
                    Id = legalCase.Id,
                    LegalCaseNumber = legalCase.LegalCaseNumber,
                    ContractId = legalCase.ContractId,
                    ContractNumber = legalCase.ContractNumber,
                    ContractDate = contractInfo.ContractDate,
                    ContractBranch = contractInfo.BranchName,
                    Client = legalCase.ContractId != null
                        ? await _legalCollectionRepository.GetClientByContractIdAsync(legalCase.ContractId)
                        : null,
                    Status = legalCase.Status,
                    Action = legalCase.Action,
                    Court = legalCase.Court,
                    Course = legalCase.Course,
                    Stage = legalCase.Stage,
                    CreateDate = legalCase.CreateDate,
                    DateStart = legalCase.StartDate,
                    EndDate = legalCase.EndDate,
                    Reason = reason?.reasonName,
                    DelayDay = legalCase.DelayCurrentDay,
                    Documents = legalCase.Documents,
                    User = user?.Fullname,
                    AccrualStopped = contract.InscriptionId.HasValue,
                    AccrualStopDate = accrualStopDate,
                    LoanPeriod = contract.LoanPeriod,
                    CaseTaskStatus = legalCase.CaseTaskStatus,
                    DaysUntilExecution = legalCase.DaysUntilExecution,
                    Car = contractInfo.Car,
                    TotalSum = totalSum > 0 ? GetCostInStringFormat(totalSum) : totalSum.ToString(CultureInfo.InvariantCulture),
                    Histories = legalCase.Histories.Select(h => new LegalCaseHistoryViewModel
                    {
                        Date = h.CreateDate,
                        Action = h.Action,
                        StageAfter = h.StageAfter,
                        DelayDay = h.DelayDays,
                        AuthorFullName = h.AuthorId != null
                            ? users.FirstOrDefault(u => u.Id == h.AuthorId)?.Fullname
                            : null,
                        Note = h.Note
                    }).ToList()
                };

                detailsLegalCase.Add(detailsViewModel);
            }

            return detailsLegalCase;
        }
        
        private string GetCostInStringFormat(decimal cost)
        {
            CultureInfo culture = new CultureInfo("ru-RU");
            if (cost % 1 == 0)
            {
                return cost.ToString("N0", culture);
            }

            return cost.ToString("N2", culture);
        }
    }
}