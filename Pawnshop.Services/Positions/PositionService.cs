using Pawnshop.Core;
using Pawnshop.Core.Extensions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Positions;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Positions
{
    public class PositionService : IPositionService
    {
        private readonly PositionRepository _positionRepository;
        private readonly IPositionSubjectService _positionSubjectService;
        private readonly PositionEstimatesRepository _positionEstimatesRepository;
        private readonly ClientRepository _clientRepository;
        private readonly IPositionEstimateHistoryService _positionEstimateHistoryService;
        //private readonly IContractService _contractService;
        private readonly ContractRepository _contractRepository;
        public PositionService(PositionRepository positionRepository, IPositionSubjectService positionSubjectService, PositionEstimatesRepository positionEstimatesRepository, ClientRepository clientRepository, 
            IPositionEstimateHistoryService positionEstimateHistoryService,
            //IContractService contractService,
            ContractRepository contractRepository
            )
        {
            _positionRepository = positionRepository;
            _positionSubjectService = positionSubjectService;
            _positionEstimatesRepository = positionEstimatesRepository;
            _clientRepository = clientRepository;
            _positionEstimateHistoryService = positionEstimateHistoryService;
            //_contractService = contractService;
            _contractRepository = contractRepository;
        }

        public async Task<PositionAdditionalInfo> GetPositionAdditionalInfo(int positionId)
        {
            var positionAdditionalInfo = new PositionAdditionalInfo();
            var estimationInfo = await _positionEstimatesRepository.GetActualPositionEstimationInformation(positionId);
            positionAdditionalInfo.EstimatedCost = estimationInfo.Item2;
            positionAdditionalInfo.PositionEstimate = estimationInfo.Item1;
            //если новая позиция для договора, то нужно будет сохранить для нее оценку
            positionAdditionalInfo.PositionEstimate.Id = 0;

            var positionSubjects = await _positionSubjectService.GetPositionSubjectsForPositionAndDate(positionId);
            if(positionSubjects.Any())
            {
                positionAdditionalInfo.PositionPledger = positionSubjects.Where(x => x.Subject.Code == Constants.MAIN_PLEDGER_CODE).FirstOrDefault().Client;
                positionAdditionalInfo.PositionSubjects = positionSubjects.Where(x => x.Subject.Code == Constants.PLEDGER_CODE).Cast<PositionSubject>().ToList();
            }
            else
            {
                positionAdditionalInfo.PositionSubjects = _positionSubjectService.GetSubjectsForPosition(positionId);
                positionAdditionalInfo.PositionPledger = await _positionRepository.GetActivePositionClient(positionId);
            }
            positionAdditionalInfo.HasUsedPledge = await HasUsedPledge(positionId);

            positionAdditionalInfo.PositionEstimateHistory = new List<PositionEstimateHistory>();
            positionAdditionalInfo.PositionEstimateHistory = await _positionEstimateHistoryService.GetPositionEstimateHistory(positionId);
            return positionAdditionalInfo;
        }

        public async Task<bool> HasUsedPledge(int positionId)
        {
            var count = await _positionRepository.GetCountForActiveContractsForPosition(positionId);
            if(count > 0)
                return true;
            return false;
        }

        public async Task<IList<ContractsWithTotalLoanCost>> GetActiveContractsForPosition(IList<int> positionIds)
        {
            var result = new List<ContractsWithTotalLoanCost>();
            foreach (var positionId in positionIds)
            {
                var contracts = new List<Contract>();
                var contractIds = await _positionRepository.GetActiveContractIdsForPositionAsync(positionId);
                foreach (var contractId in contractIds)
                {
                    contracts.Add(_contractRepository.GetOnlyContract(contractId));
                }

                var resultItem = new ContractsWithTotalLoanCost();
                resultItem.Contracts = contracts;
                resultItem.CalculateTotalCost();
                resultItem.PositionId = positionId;
                result.Add(resultItem);
            }

            return result;
        }
    }
}
