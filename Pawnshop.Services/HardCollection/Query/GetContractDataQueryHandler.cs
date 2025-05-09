using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Services.HardCollection.Query
{
    public class GetContractDataQueryHandler : IRequestHandler<GetContractDataQuery, ContractData>
    {
        private readonly IMediator _mediator;
        private readonly ClientRepository _clientRepository;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly ClientAdditionalContactRepository _clientAdditionalContactRepository;
        private readonly ContractRepository _contractRepository;
        private readonly CarRepository _carRepository;

        public GetContractDataQueryHandler(IMediator mediator, 
            ClientRepository clientRepository, 
            ClientContactRepository clientContactRepository, 
            ClientAdditionalContactRepository clientAdditionalContactRepository, 
            ContractRepository contractRepository, 
            CarRepository carRepository)
        {
            _mediator = mediator;
            _clientRepository = clientRepository;
            _clientContactRepository = clientContactRepository;
            _clientAdditionalContactRepository = clientAdditionalContactRepository;
            _contractRepository = contractRepository;
            _carRepository = carRepository;
        }

        public async Task<ContractData> Handle(GetContractDataQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                var contract = await _contractRepository.GetOnlyContractAsync(query.ContractId);
                if (contract.CollateralType != CollateralType.Car)
                    return null;

                var hcContract = await _mediator.Send(new GetContractOnlyQuery() { ContractId = query.ContractId, IsJobWorking = query.IsJobWorking });

                var model = new ContractData
                {
                    Contract = hcContract.Contract,
                    ContractClient = await GetClientDataAsync(contract.ClientId),
                    ContractCar = await GetCarDataAsync(query.ContractId)
                };

                return model;
            }
            catch(Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = query.GetLogNotifications($"Ошибка при получений полных данных договора по ContractId={query.ContractId}", ex, Data.Models.Audit.EventStatus.Failed, !query.IsJobWorking, false)
                };
                await _mediator.Publish(notification);

                throw ex;
            }
        }

        private async Task<CarData> GetCarDataAsync(int contractId)
        {
            var contract = await _contractRepository.GetOnlyContractAsync(contractId);
            if (contract.ContractClass == Data.Models.Contracts.ContractClass.Tranche)
                contractId = await _contractRepository.GetCreditLineId(contractId);

            var car = await _carRepository.GetByContractIdAsync(contractId);

            return new CarData
            {
                BodyNumber = car.BodyNumber,
                TransportNumber = car.TransportNumber,
                Mark = car.Mark,
                Model = car.Model,
                ParkingStatusId = car.ParkingStatusId.HasValue ? car.ParkingStatusId.Value : 0,
                ReleaseYear = car.ReleaseYear
            };
        }

        private async Task<ClientData> GetClientDataAsync(int clientId)
        {
            var client = await _clientRepository.GetOnlyClientAsync(clientId);
            var clientAddressList = await _clientRepository.GetClientAddressesAsync(clientId);
            var clientContactList = await _clientContactRepository.ListAsync(new Core.Queries.ListQuery(), new { ClientId = clientId });
            var clientAdditionalContactList = await _clientAdditionalContactRepository.GetListByClientIdAsync(clientId);

            return new ClientData
            {
                IIN = client.IdentityNumber,
                FullName = client.FullName,
                AdditionalContactList = clientAdditionalContactList.ToArray(),
                ContactList = clientContactList.ToArray(),
                AddressList = clientAddressList.ToArray(),
            };
        }
    }
}
