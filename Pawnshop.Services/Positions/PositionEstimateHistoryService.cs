using Microsoft.AspNetCore.Mvc.TagHelpers;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Positions
{
    public class PositionEstimateHistoryService : IPositionEstimateHistoryService
    {
        private readonly PositionEstimateHistoryRepository _positionEstimateHistoryRepository;
        private readonly PositionEstimatesRepository _positionEstimatesRepository;
        private readonly RealtyRepository _realtyRepository;
        private readonly ContractRepository _contractRepository;
        private readonly DomainValueRepository _domainValueRepository;

        public PositionEstimateHistoryService(PositionEstimateHistoryRepository positionEstimateHistoryRepository, PositionEstimatesRepository positionEstimatesRepository, 
            RealtyRepository realtyRepository, ContractRepository contractRepository, DomainValueRepository domainValueRepository)
        { 
            _positionEstimateHistoryRepository = positionEstimateHistoryRepository;
            _positionEstimatesRepository = positionEstimatesRepository;
            _realtyRepository = realtyRepository;
            _contractRepository = contractRepository;
            _domainValueRepository = domainValueRepository;
        }

        public async Task<List<PositionEstimateHistory>> GetPositionEstimateHistory(int positionId)
        {
            var result = _positionEstimateHistoryRepository.ListEstimationHistoryForPosition(positionId);
            if(!result.Any())
            {
                var oldEstimationsInContractPositionsForm = await _positionEstimatesRepository.GetActualContractPositionsWithEstimateForPosition(positionId);
                foreach (var contractPositionForm in oldEstimationsInContractPositionsForm)
                {
                    var historyItem = await ConvertLegacyContractPositionEstimateToHistoryItem(contractPositionForm);
                    if(!await IsHistoryItemInHistoryAlready(result, historyItem))
                        result.Add(historyItem);
                }
            }
            return result;
        }

        public async Task SavePositionEstimateToHistoryForContract(Contract contract)
        {
            foreach (var position in contract.Positions)
            {
                if (position.Position.CollateralType != Pawnshop.AccountingCore.Models.CollateralType.Realty)
                    continue;

                var positionEstimateHistory = _positionEstimateHistoryRepository.ListEstimationHistoryForPosition(position.PositionId);

                // если в истории оценок нет оценок, то берем оценки, которые были созданы до создания функционала по истории, и заполняем их для истории и сохраняем в историю
                // => постепенная миграяция оценок для старых договоров
                if (!positionEstimateHistory.Any())
                {
                    var oldEstimationsInContractPositionsForm = await _positionEstimatesRepository.GetActualContractPositionsWithEstimateForPosition(position.PositionId);
                    foreach (var contractPositionForm in oldEstimationsInContractPositionsForm)
                    {
                        var historyItem = await ConvertLegacyContractPositionEstimateToHistoryItem(contractPositionForm);
                        if(!await IsHistoryItemInHistoryAlready(positionEstimateHistory, historyItem))
                        {
                            positionEstimateHistory.Add(historyItem);
                            _positionEstimateHistoryRepository.Insert(historyItem);
                        }
                    }
                }

                if (positionEstimateHistory.Any())
                {
                    if (await IsEstimateInHistoryAlready(positionEstimateHistory, position))
                    {
                        continue;
                    }
                }
                var newEstimateHistoryItem = await ConvertContractPositionToPositionEstimateHistoryItem(contract, position);
                _positionEstimateHistoryRepository.Insert(newEstimateHistoryItem);

            }
        }

        public async Task ValidateDateAndEstimatedCost(Contract contract)
        {
            foreach (var position in contract.Positions)
            {
                if (position.Position.CollateralType != Pawnshop.AccountingCore.Models.CollateralType.Realty)
                    continue;

                var positionEstimateHistory = _positionEstimateHistoryRepository.ListEstimationHistoryForPosition(position.PositionId);

                if (!positionEstimateHistory.Any())
                {
                    var oldEstimationsInContractPositionsForm = await _positionEstimatesRepository.GetActualContractPositionsWithEstimateForPosition(position.PositionId);
                    foreach (var contractPositionForm in oldEstimationsInContractPositionsForm)
                    {
                        var historyItem = await ConvertLegacyContractPositionEstimateToHistoryItem(contractPositionForm);
                        if (!await IsHistoryItemInHistoryAlready(positionEstimateHistory, historyItem))
                            positionEstimateHistory.Add(historyItem);
                    }
                }

                if (positionEstimateHistory.Any())
                {
                    await ValidateDateAndEstimatedCost(positionEstimateHistory, position);
                }
            }
        }

        private async Task<bool> IsEstimateInHistoryAlready(List<PositionEstimateHistory> positionEstimates, ContractPosition position)
        {
            foreach (var estimateHistoryItem in positionEstimates)
            {
                if (estimateHistoryItem.Date == position.PositionEstimate.Date
                            && estimateHistoryItem.CollateralCost == position.CollateralCost
                            && estimateHistoryItem.CompanyId == position.PositionEstimate.CompanyId
                            && estimateHistoryItem.EstimatedCost == position.EstimatedCost
                            && estimateHistoryItem.Number == position.PositionEstimate.Number)
                    return true;
            }
            return false;
        }

        private async Task<bool> IsHistoryItemInHistoryAlready(List<PositionEstimateHistory> estimateHistory, PositionEstimateHistory itemForCheck)
        {
            foreach (var estimateHistoryItem in estimateHistory)
            {
                if (estimateHistoryItem.Date == itemForCheck.Date
                            && estimateHistoryItem.CollateralCost == itemForCheck.CollateralCost
                            && estimateHistoryItem.CompanyId == itemForCheck.CompanyId
                            && estimateHistoryItem.EstimatedCost == itemForCheck.EstimatedCost
                            && estimateHistoryItem.Number == itemForCheck.Number)
                    return true;
            }
            return false;
        }

        private async Task ValidateDateAndEstimatedCost(List<PositionEstimateHistory> positionEstimates, ContractPosition position)
        {
            foreach(var estimate in positionEstimates)
            {
                if (estimate.Date == position.PositionEstimate.Date && estimate.EstimatedCost != position.EstimatedCost)
                    throw new PawnshopApplicationException($"В истории оценок для позиции {position.Position.Name} найдена запись - изменена сумма оценки, но не изменена дата оценки");
            }
        }



        private async Task<PositionEstimateHistory> ConvertContractPositionToPositionEstimateHistoryItem(Contract contract, ContractPosition position)
        {
            var result = new PositionEstimateHistory();
            result.PositionId = position.PositionId;
            result.CollateralCost = position.CollateralCost ?? throw new PawnshopApplicationException($"Для заполнения истории не подсчитана залоговая стоимость для позиции {position.Position.Name}");
            result.Date = position.PositionEstimate?.Date ?? throw new PawnshopApplicationException($"Для заполнения истории не найдена дата для позиции {position.Position.Name} - PositionEstimateHistoryService - ConvertContractPositionToPositionEstimateHistoryItem");
            result.EstimatedCost = position.EstimatedCost;
            result.CompanyId = position.PositionEstimate?.CompanyId ?? throw new PawnshopApplicationException($"Для заполнения истории не найдена оценочная компания для старых оценок - позиция {position.Position.Name} - PositionEstimateHistoryService - ConvertContractPositionToPositionEstimateHistoryItem");
            result.Number = position.PositionEstimate?.Number;
            result.BeginDate = contract.SignDate ?? DateTime.Today;
            result.CreateDate = DateTime.Now;
            result.AuthorId = contract.AuthorId;
            return result;
        }

        //для постепенной миграции истории оценок договоров
        private async Task<PositionEstimateHistory> ConvertLegacyContractPositionEstimateToHistoryItem(ContractPosition position)
        {
            var signDate = position.Contract.SignDate;
            var contractDate = position.Contract.ContractDate;
            var result = new PositionEstimateHistory();
            result.PositionId = position.PositionId;
            result.CollateralCost = position.CollateralCost ?? await GetCollateralCostForLegacyProducts(position);
            result.Date = position.PositionEstimate?.Date ?? throw new PawnshopApplicationException($"Для заполнения истории не найдена дата оценки для старых оценок - позиция {position.Position.Name}- PositionEstimateHistoryService - ConvertLegacyContractPositionEstimateToHistoryItem");
            result.EstimatedCost = position.EstimatedCost;
            result.CompanyId = position.PositionEstimate?.CompanyId ?? throw new PawnshopApplicationException($"Для заполнения истории не найдена оценочная компания для старых оценок - позиция {position.Position.Name} - PositionEstimateHistoryService - ConvertLegacyContractPositionEstimateToHistoryItem");
            result.Company = position.PositionEstimate?.Company;
            result.Number = position.PositionEstimate?.Number;
            result.BeginDate = signDate ?? contractDate;
            result.CreateDate = DateTime.Now;
            result.AuthorId = position.PositionEstimate.AuthorId;

            return result;
        }

        //для постепенной миграции истории оценок договоров
        private async Task<decimal> GetCollateralCostForLegacyProducts(ContractPosition position)
        {

            var collateralCost = Decimal.Zero;
            
            if(position.Position.CollateralType != Pawnshop.AccountingCore.Models.CollateralType.Realty)
                return collateralCost;

            var realty = _realtyRepository.Get(position.Id);
            realty.RealtyType = _domainValueRepository.Get(realty.RealtyTypeId);
            switch (realty.RealtyType.Code)
            {
                case Constants.REALTY_APPARMENT:
                case Constants.REALTY_COMMERCIAL_PROPERTY:
                case Constants.REALTY_HOUSE:
                    collateralCost = Decimal.Multiply((decimal)position.EstimatedCost, (decimal) 0.5);
                    break;
                case Constants.REALTY_COMMERCIAL_LAND:
                case Constants.REALTY_PARKING:
                    collateralCost = Decimal.Multiply((decimal)position.EstimatedCost, (decimal) 0.3);
                    break;
                default:
                    throw new PawnshopApplicationException($"Для типа недвижимости {realty.RealtyType.Code} не настроен подсчет залоговой стоимости в легаси.");
            }

            return collateralCost;
        }
    }
}
