using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.AccountingCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net.WebSockets;
using Pawnshop.Data.Models.Parking;
using System.Threading.Tasks;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Cars
{
    public class ParkingActionService : IParkingActionService
    {
        private readonly ParkingStatusRepository _parkingStatusRepository;
        private readonly ParkingActionRepository _parkingActionRepository;
        private readonly ISessionContext _sessionContext;
        private readonly ParkingHistoryRepository _parkingHistoryRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly CarRepository _carRepository;
        public ParkingActionService(ParkingActionRepository parkingActionRepository
            , ISessionContext sessionContext
            , ParkingStatusRepository parkingStatusRepository
            , ParkingHistoryRepository parkingHistoryRepository
            , ContractActionRepository contractActionRepository
            , CarRepository carRepository)
        {
            _parkingActionRepository = parkingActionRepository;
            _sessionContext = sessionContext;
            _parkingStatusRepository = parkingStatusRepository;
            _parkingHistoryRepository = parkingHistoryRepository;
            _contractActionRepository = contractActionRepository;
            _carRepository = carRepository; 
        }
        public ParkingHistory BuyoutNewParkingHistory(Contract contract)
        {
            if (!ValidateContract(contract))
                return null;

            var position = contract.Positions.FirstOrDefault().Position as Car;
            var parkingStatusList = _parkingStatusRepository.List(new Core.Queries.ListQuery());
            var category = contract.Positions?.FirstOrDefault()?.Category;

            ParkingHistory parkingHistory = new ParkingHistory()
            {
                ContractId = contract.Id,
                CreateDate = DateTime.Now,
                Date = DateTime.Now.Date,
                PositionId = position.Id,
                StatusBeforeId = position.ParkingStatusId,
                UserId = _sessionContext.UserId
            };
            var case1 = parkingStatusList.Where(x => x.StatusCode == Constants.INPARKING_AUCTION
                                    || x.StatusCode == Constants.TRANSFERRED_CARTAS
                                    || x.StatusCode == Constants.INPARKING_SELLING)
                                .Select(x => x.Id).ToList().Contains((int)position.ParkingStatusId);

            var case2 = parkingStatusList.Where(x => x.StatusCode == Constants.INPARKING_WAITING)
                                    .Select(x => x.Id).ToList().Contains((int)position.ParkingStatusId);
            if (case1)
            {
                var parkingStatusAfter = parkingStatusList.Where(x => x.StatusCode == Constants.TRANSFERRED_BUYER).FirstOrDefault();
                var parkingAction = _parkingActionRepository.List(new Core.Queries.ListQuery(),
                    new
                    {
                        StatusBeforeId = position.ParkingStatusId,
                        StatusAfterId = parkingStatusAfter.Id,
                        ActionName = "Передать покупателю",
                        CollateralType = CollateralType.Car,
                        CategoryId = contract.Positions?.FirstOrDefault()?.Category.Id
                    }).FirstOrDefault();

                parkingHistory.ParkingActionId = parkingAction.Id;
                parkingHistory.StatusAfterId = parkingStatusAfter.Id;
                return parkingHistory;
            }
            else if (case2)
            {
                var parkingStatusAfter = parkingStatusList.Where(x => x.StatusCode == Constants.AT_CLIENT).FirstOrDefault();
                var parkingAction = _parkingActionRepository.List(new Core.Queries.ListQuery(),
                    new
                    {
                        StatusBeforeId = position.ParkingStatusId,
                        StatusAfterId = parkingStatusAfter.Id,
                        ActionName = "Вернуть клиенту",
                        CollateralType = CollateralType.Car,
                        CategoryId = category.Id
                    }).FirstOrDefault();

                parkingHistory.ParkingActionId = parkingAction.Id;
                parkingHistory.StatusAfterId = parkingStatusAfter.Id;
                return parkingHistory;
            }
            return null;
        }
        public async Task CancelParkingHistory(Contract contract)
        {
            if (!ValidateContract(contract))
                return;

            var position = contract.Positions.FirstOrDefault().Position as Car;
            if (position is Car car)
            {
                var lastParkingHistoryItem =  _parkingHistoryRepository.GetActiveLastByContractId(contract.Id);

                if (lastParkingHistoryItem == null || lastParkingHistoryItem.ActionId == null)
                    return;

                car.ParkingStatusId = lastParkingHistoryItem.StatusBeforeId;

                _parkingHistoryRepository.Delete(lastParkingHistoryItem.Id);
                _carRepository.Update(car);
            }
        }
        private bool ValidateContract(Contract contract)
        {
            if (contract.ContractClass == ContractClass.Tranche)
                return false;

            var position = contract.Positions?.FirstOrDefault()?.Position;
            if (position == null)
                return false;

            var category = contract.Positions?.FirstOrDefault()?.Category;
            if (category == null)
                return false;

            if (position is Car car)
                if (car.ParkingStatusId == null)
                    return false;

            if (contract.CollateralType != CollateralType.Car)
                return false;

            return true;
        }
        public void ChangeParkingStatusByCategory(Contract contract, int actionId)
        {
            var position = contract.Positions.FirstOrDefault(x => x.Position.CollateralType == CollateralType.Car);
            if (position == null)
                return;

            var maxlimit = new Core.Queries.ListQuery();
            maxlimit.Page.Limit = 10000;
            var parkAction = _parkingActionRepository.List(maxlimit);
            

            if (position.Category.Code == Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE)
            {
                parkAction = parkAction.Where(x => x.StatusBefore.StatusCode == Constants.INPARKING_WAITING && x.StatusAfter.StatusCode == Constants.AT_CLIENT &&
                                                            x.CategoryId == position.CategoryId).ToList();
            }
            else if (position.Category.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE)
            {
                parkAction = parkAction.Where(x => x.StatusBefore.StatusCode == Constants.AT_CLIENT && x.StatusAfter.StatusCode == Constants.INPARKING_WAITING &&
                                                            x.CategoryId == position.CategoryId).ToList();
            }
                

            

            ParkingHistory history = new ParkingHistory()
            {
                ContractId = contract.Id,
                PositionId = position.Position.Id,
                StatusBeforeId = parkAction.FirstOrDefault().StatusBeforeId,
                StatusAfterId = parkAction.FirstOrDefault().StatusAfterId,
                UserId = _sessionContext.UserId,
                DelayDays = contract.DelayDays,
                CreateDate = DateTime.Now,
                Date = DateTime.Now,
                ParkingActionId = parkAction.FirstOrDefault().Id,
                ActionId = actionId
            };
            _parkingHistoryRepository.Insert(history);

            var car = contract.Positions.FirstOrDefault(x => x.Position.CollateralType == CollateralType.Car).Position as Car;
            car.ParkingStatusId = parkAction.FirstOrDefault().StatusAfterId;
            _carRepository.Update(car);
        }
           public void UpdateParkingHistory( int authorId, bool isChecked, int parentActionId, Contract contract)
        {
            var allParkingStatuses = _parkingStatusRepository.List(new Core.Queries.ListQuery());
            var position = contract.Positions.FirstOrDefault()?.Position as Car;
            if (position is null)
            {
                return;
            }
            var lastParkingHistory = _parkingHistoryRepository.GetDeletedByContractId(contract.Id);
            if (lastParkingHistory is null)
            {
                return;
            }
            
            var statusBeforeId = lastParkingHistory.DeleteDate.HasValue ? lastParkingHistory.StatusBeforeId : lastParkingHistory.StatusAfterId;
            var parkingHistory = new ParkingHistory
            {
                ContractId = contract.Id,
                CreateDate = DateTime.Now,
                Date = DateTime.Now.Date,
                UserId = authorId,
                PositionId = position.Id,
                StatusBeforeId = statusBeforeId,
                ActionId = parentActionId == default ? (int?)null : parentActionId
            };
            var category = contract.Positions?.FirstOrDefault()?.Category;

            if (statusBeforeId == allParkingStatuses.First(x => x.StatusCode == Constants.INPARKING_WAITING).Id)
            {
                if (contract.ContractClass == ContractClass.Credit)
                {
                    UpdateParkingStatus(allParkingStatuses, parkingHistory, position, Constants.AT_CLIENT, statusBeforeId, category?.Id ?? default, "Передать покупателю");
                }
                else if (isChecked)
                {
                    UpdateParkingStatus(allParkingStatuses, parkingHistory, position, Constants.AT_CLIENT, statusBeforeId, category?.Id ?? default, "Передать покупателю");
                }
            }
            else if (IsStatusInList(statusBeforeId, new[] { Constants.INPARKING_LEGAL, Constants.INPARKING_AUCTION, Constants.TRANSFERRED_CARTAS }, allParkingStatuses))
            {
                if (!isChecked && contract.ContractClass == ContractClass.CreditLine)
                {
                    throw new PawnshopApplicationException("Checkbox на закрытие кредитной линии не был отмечен.");
                }
                UpdateParkingStatus(allParkingStatuses, parkingHistory, position, Constants.TRANSFERRED_BUYER, statusBeforeId, category?.Id ?? default, "Вернуть клиенту");
            }
        }

        private bool IsStatusInList(int? statusId, string[] statusCodes, List<ParkingStatus> parkingStatusList)
        {
            if (statusId == null)
            {
                return false;
            }

            return parkingStatusList.Any(x => x.Id == statusId.Value && statusCodes.Contains(x.StatusCode));
        }

        private void UpdateParkingStatus(
            List<ParkingStatus> parkingStatusList, 
            ParkingHistory parkingHistory, 
            Car position, 
            string statusAfterCode, 
            int? statusBeforeId,
            int categoryId,
            string actionName)
        {
            var parkingStatusAfter = parkingStatusList.First(x => x.StatusCode == statusAfterCode);
            var parkingAction = _parkingActionRepository.List(new Core.Queries.ListQuery(), new
                {
                    StatusBeforeId = position.ParkingStatusId,
                    StatusAfterId = parkingStatusAfter.Id,
                    ActionName = actionName,
                    CollateralType = CollateralType.Car,
                    CategoryId = categoryId
                }).FirstOrDefault();
            parkingHistory.StatusAfterId = parkingStatusAfter.Id;
            parkingHistory.StatusBeforeId = statusBeforeId;
            parkingHistory.ParkingActionId = parkingAction?.Id ?? null;
            position.ParkingStatusId = parkingStatusAfter.Id;
            _parkingHistoryRepository.Insert(parkingHistory); 
            _carRepository.Update(position);
        }
    }
}
