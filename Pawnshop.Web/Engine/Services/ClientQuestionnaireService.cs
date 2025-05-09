using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Clients;

namespace Pawnshop.Web.Engine.Services
{
    public class ClientQuestionnaireService : IClientQuestionnaireService
    {
        private readonly IClientAdditionalContactService _clientAdditionalContactService;
        private readonly IClientProfileService _clientProfileService;
        private readonly IClientExpenseService _clientExpenseService;
        private readonly IClientEmploymentService _clientEmploymentService;
        private readonly IClientAssetService _clientAssetService;
        private readonly IClientService _clientService;
        public const int REQUIRED_ADDITIONAL_CONTACT_COUNT = 2;
        public ClientQuestionnaireService(IClientAdditionalContactService clientAdditionalContactService, IClientExpenseService clientExpenseService,
            IClientProfileService clientProfileService, IClientEmploymentService clientEmploymentService,
            IClientService clientService, IClientAssetService clientAssetService)
        {
            _clientAdditionalContactService = clientAdditionalContactService;
            _clientProfileService = clientProfileService;
            _clientExpenseService = clientExpenseService;
            _clientEmploymentService = clientEmploymentService;
            _clientService = clientService;
            _clientAssetService = clientAssetService;
        }

        public bool IsClientHasFilledQuestionnaire(int clientId)
        {
            Client client = _clientService.Get(clientId);
            if (client == null)
                throw new PawnshopApplicationException($"Клиент {clientId} не существует");

            if (client.LegalForm == null)
                throw new PawnshopApplicationException($"Не найдена информация является ли Клиент {client.FullName} юридическим лицом");

            bool isIndividual = client.LegalForm.IsIndividual;
            if (!isIndividual)
                return !string.IsNullOrWhiteSpace(client.CodeWord);

            ClientQuestionnaireFilledStatusDto status = GetClientQuestionnaireFilledStatus(clientId);
            return status.IsProfileFilled && status.IsExpenseFilled
                && status.AreEmploymentsFilled && status.AreAdditionalContactsFilled
                && status.IsCodeWordFilled && status.AreAssetsFilled;
        }


        private ClientQuestionnaireFilledStatusDto GetClientQuestionnaireFilledStatus(int clientId)
        {
            Client client = _clientService.Get(clientId);
            if (client == null)
                throw new PawnshopApplicationException($"Клиент {clientId} не существует");

            if (client.LegalForm == null)
                throw new PawnshopApplicationException($"Не найдена информация является ли Клиент {client.FullName} юридическим лицом");

            bool isIndividual = client.LegalForm.IsIndividual;
            if (!isIndividual)
                throw new PawnshopApplicationException($"Клиент {client.FullName} должен быть физическим лицом");

            List<ClientAdditionalContact> contacts = _clientAdditionalContactService.Get(clientId);
            var status = new ClientQuestionnaireFilledStatusDto
            {
                IsExpenseFilled = _clientExpenseService.IsClientExpenseFilled(clientId),
                RequiredAdditionalContactsCount = REQUIRED_ADDITIONAL_CONTACT_COUNT,
                AdditionalContactsCount = contacts.Count,
                IsCodeWordFilled = !string.IsNullOrWhiteSpace(client.CodeWord)
            };

            ClientProfile profile = _clientProfileService.Get(clientId);
            if (profile != null)
            {
                status.IsProfileFilled = _clientProfileService.IsClientProfileFilled(clientId);
                // если клиент работает, то у него должна быть хотя бы одна работа
                if (profile.IsWorkingNow.HasValue)
                {
                    status.AreEmploymentsFilled = true;
                    if (profile.IsWorkingNow.Value)
                    {
                        List<ClientEmployment> employments = _clientEmploymentService.Get(clientId);
                        status.AreEmploymentsFilled = employments.Count > 0;
                    }
                }

                if (profile.HasAssets.HasValue)
                {
                    status.AreAssetsFilled = true;
                    if (profile.HasAssets.Value)
                    {
                        List<ClientAsset> assets = _clientAssetService.Get(clientId);
                        status.AreAssetsFilled = assets.Count > 0;
                    }
                }
            }

            return status;
        }

        public bool CanFillQuestionnaire(int clientId)
        {
            Client client = _clientService.Get(clientId);
            if (client == null)
                throw new PawnshopApplicationException($"Клиент {clientId} не найден");

            if (client.LegalForm == null)
                throw new PawnshopApplicationException($"Не найдена информация о принадлежности клиента {client.FullName} к юридическому лицу");

            return client.LegalForm.IsIndividual;
        }
    }
}
