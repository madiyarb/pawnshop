using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Parking;
using System;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.Collection;
using System.Linq;

namespace Pawnshop.Services.Parkings.History
{
    public class ParkingHistoryService : IParkingHistoryService
    {
        private readonly IAbsOnlineService _absOnlineService;
        private readonly CarRepository _carRepository;
        private readonly ICollectionService _collectionService;
        private readonly ContractRepository _contractRepository;
        private readonly MachineryRepository _machineryRepository;
        private readonly ParkingActionRepository _parkingActionRepository;
        private readonly ParkingHistoryRepository _parkingHistoryRepository;
        private readonly ISessionContext _sessionContext;

        public ParkingHistoryService(
            IAbsOnlineService absOnlineService,
            CarRepository carRepository,
            ICollectionService collectionService,
            ContractRepository contractRepository,
            MachineryRepository machineryRepository,
            ParkingActionRepository parkingActionRepository,
            ParkingHistoryRepository parkingHistoryRepository,
            ISessionContext sessionContext
            )
        {
            _absOnlineService = absOnlineService;
            _carRepository = carRepository;
            _collectionService = collectionService;
            _contractRepository = contractRepository;
            _machineryRepository = machineryRepository;
            _parkingActionRepository = parkingActionRepository;
            _parkingHistoryRepository = parkingHistoryRepository;
            _sessionContext = sessionContext;
        }

        public ParkingHistory Save(ParkingHistory parkingHistory)
        {
            if (parkingHistory == null)
                throw new ArgumentNullException(nameof(parkingHistory));

            if (parkingHistory.ContractId <= 0)
                throw new ArgumentException();

            if (!(parkingHistory.ParkingActionId.HasValue && parkingHistory.ParkingActionId.Value > 0))
                throw new PawnshopApplicationException($"Нужно выбрать действие");

            var contract = _contractRepository.GetOnlyContract(parkingHistory.ContractId);
            if(contract.ContractClass == Data.Models.Contracts.ContractClass.Tranche)
            {
                var creditLineId = _contractRepository.GetCreditLineByTrancheId(contract.Id).Result;
                contract = _contractRepository.Get(creditLineId);
                parkingHistory.ContractId = creditLineId;
                parkingHistory.Contract = contract;
            }
            else
            {
                contract = _contractRepository.Get(parkingHistory.ContractId);
            }

            if (contract.Status == ContractStatus.Draft)
                throw new PawnshopApplicationException($"Договор #{contract.ContractNumber} не Подписан");

            if (!(contract.CollateralType != CollateralType.Car || contract.CollateralType != CollateralType.Machinery))
                throw new PawnshopApplicationException($"Недопустимый вид залога");

            using (var transaction = _parkingHistoryRepository.BeginTransaction())
            {
                var parkingAction = _parkingActionRepository.Get((int)parkingHistory.ParkingActionId);

                if (contract.CollateralType == CollateralType.Car)
                {
                    var position = contract.Positions.FirstOrDefault().Position as Car;

                    if (position.ParkingStatusId != parkingAction.StatusBeforeId)
                        throw new PawnshopApplicationException($"На позиции другой статус стоянки");

                    position.ParkingStatusId = parkingAction.StatusAfterId;

                    _carRepository.Update(position);

                    parkingHistory.PositionId = position.Id;
                }
                else if (contract.CollateralType == CollateralType.Machinery)
                {
                    var position = contract.Positions.FirstOrDefault().Position as Machinery;

                    if (position.ParkingStatusId != parkingAction.StatusBeforeId)
                        throw new PawnshopApplicationException($"На позиции другой статус стоянки");

                    position.ParkingStatusId = parkingAction.StatusAfterId;

                    _machineryRepository.Update(position);

                    parkingHistory.PositionId = position.Id;
                }

                parkingHistory.StatusBeforeId = parkingAction.StatusBeforeId;
                parkingHistory.StatusAfterId = parkingAction.StatusAfterId;
                parkingHistory.UserId = _sessionContext.UserId;

                _parkingHistoryRepository.Insert(parkingHistory);

                transaction.Commit();

                if (parkingAction.StatusAfter.StatusCode == Constants.INPARKING_WAITING)
                {
                    var contractClass = _contractRepository.GetOnlyContract(parkingHistory.ContractId).ContractClass;
                    if(contractClass == Data.Models.Contracts.ContractClass.Tranche)
                    {
                        var tempCreditLineId = _contractRepository.GetCreditLineByTrancheId(parkingHistory.ContractId).Result;
                        var trancheList = _contractRepository.GetTrancheIdsByCreditLine(tempCreditLineId).Result;
                        foreach(var id in trancheList)
                        {
                            _collectionService.ParkingChangeStatus(id);
                        }
                    }
                    else if(contractClass == Data.Models.Contracts.ContractClass.Credit)
                    {
                        _collectionService.ParkingChangeStatus(parkingHistory.ContractId);
                    }
                    else if(contractClass == Data.Models.Contracts.ContractClass.CreditLine)
                    {
                        var trancheList = _contractRepository.GetTrancheIdsByCreditLine(parkingHistory.ContractId).Result;
                        foreach (var id in trancheList)
                        {
                            _collectionService.ParkingChangeStatus(id);
                        }
                    }
                }

                _absOnlineService.SendNotificationCreditLineChangedAsync(contract.Id, contract).Wait();

                return parkingHistory;
            }
        }
    }
}
