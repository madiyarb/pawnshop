using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ClientExternalValidationData;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.DebtorRegistry;
using Pawnshop.Services.SUSN;
using Pawnshop.Services.TasLabBankrupt;
using Serilog;
using Pawnshop.Core;

namespace Pawnshop.Services.ClientExternalValidation
{
    public sealed class ClientExternalValidationService : IClientExternalValidationService
    {
        private readonly ClientExternalValidationDataRepository _clientExternalValidationDataRepository;
        private readonly ITasLabSUSNService _susnService;
        private readonly IDebtorRegistryService _debtorRegistryService;
        private readonly ITasLabBankruptInfoService _bankruptInfoService;
        private readonly SUSNRequestsRepository _susnRequestsRepository;
        private readonly ClientSUSNStatusesRepository _clientSusnStatusesRepository;
        private readonly ILogger _logger;

        public ClientExternalValidationService(ClientExternalValidationDataRepository clientExternalValidationDataRepository,
            ITasLabSUSNService susnService,
            IDebtorRegistryService debtorRegistryService,
            ITasLabBankruptInfoService bankruptInfoService,
            SUSNRequestsRepository susnRequestsRepository,
            ClientSUSNStatusesRepository clientSusnStatusesRepository,
            ILogger logger)
        {
            _clientExternalValidationDataRepository = clientExternalValidationDataRepository;
            _susnService = susnService;
            _debtorRegistryService = debtorRegistryService;
            _bankruptInfoService = bankruptInfoService;
            _susnRequestsRepository = susnRequestsRepository;
            _clientSusnStatusesRepository = clientSusnStatusesRepository;
            _logger = logger;
        }

        public async Task<bool> CheckClientForExternalIntegration(Client client, CancellationToken cancellationToken)
        {
            ClientExternalValidationData data =
                new ClientExternalValidationData(Guid.NewGuid(), client.Id, DateTime.Now);

            #region DebtorRegistry

            try
            {
                await _debtorRegistryService.GetInfoFromDebtorRegistry(client.IdentityNumber, client.Id,
                    cancellationToken);
                data.DebtorRegistryValidation = true;
            }
            catch (Exception exception)
            {
                data.DebtorRegistryValidation = false;
                _logger.Error(exception, exception.Message);
            }

            #endregion
            #region Bankrupt
            try
            {
                var isClientBankrupt = await
                    _bankruptInfoService.IsClientBankruptFromDatabase(client.IdentityNumber, cancellationToken);
                if (isClientBankrupt)
                    return false;
                isClientBankrupt =
                    await _bankruptInfoService.IsClientBankruptOnline(client.IdentityNumber, cancellationToken);
                if (isClientBankrupt)
                    return false;

                data.BankruptValidation = true;

            }
            catch (Exception exception)
            {
                data.BankruptValidation = false;
                _logger.Error(exception, exception.Message);
            }
            #endregion
            #region SUSN
            try
            { 
                var susnRequest = await _susnRequestsRepository.GetLastRequestByClientId(client.Id);
                if (susnRequest != null && susnRequest.CreateDate.Date == DateTime.Now.Date)
                {
                    var susnStatuses = await _clientSusnStatusesRepository.GetStatusesView(client.Id, susnRequest.Id);
                    if (susnStatuses != null && susnStatuses.AnySUSNStatus && susnStatuses.List.Any(status => status.Permanent))
                    {
                        return false;
                    }
                }
                else
                {
                    await _susnService.GetSUSNStatus(client.IdentityNumber, client.Id, cancellationToken);
                    susnRequest = await _susnRequestsRepository.GetLastRequestByClientId(client.Id);
                    if (susnRequest != null)
                    {
                        var susnStatuses = await _clientSusnStatusesRepository.GetStatusesView(client.Id, susnRequest.Id);
                        if (susnStatuses != null && susnStatuses.AnySUSNStatus && susnStatuses.List.Any(status => status.Permanent))
                        {
                            return false;
                        }
                    }
                }
                data.SUSNValidation = true;
            }
            catch (Exception exception)
            {
                data.SUSNValidation = false;
                _logger.Error(exception, exception.Message);
            }
            #endregion

            await _clientExternalValidationDataRepository.Insert(data);
            return true;
        }
    }
}
