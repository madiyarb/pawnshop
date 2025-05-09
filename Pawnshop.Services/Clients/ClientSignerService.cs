using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Clients
{
    public class ClientSignerService : BaseService<ClientSigner>, IClientSignerService
    {
        private readonly IBaseService<ClientSignersAllowedDocumentType> _clientSignersAllowedDocumentTypeService;
        private readonly ClientRepository _clientRepository;
        private readonly IClientService _clientService;
        private readonly IDomainService _domainService;
        private readonly ISessionContext _sessionContext;
        private readonly IClientModelValidateService _clientModelValidateService;

        public ClientSignerService(IRepository<ClientSigner> repository,
            ClientRepository clientRepository,
            IClientService clientService,
            IDomainService domainService,
            ISessionContext sessionContext,
            IBaseService<ClientSignersAllowedDocumentType> clientSignersAllowedDocumentTypeService,
            IClientModelValidateService clientModelValidateService) : base(repository)
        {
            _clientSignersAllowedDocumentTypeService = clientSignersAllowedDocumentTypeService;
            _clientRepository = clientRepository;
            _clientService = clientService;
            _domainService = domainService;
            _sessionContext = sessionContext;
            _clientModelValidateService = clientModelValidateService;
        }

        public ClientSigner CheckClientSigner(Client client, DateTime contractDate, int? signerId)
        {
            var clientSignersAllowedDocumentTypes = GetClientSignersAllowedDocumentTypes(client.LegalFormId);

            ClientSigner clientSigner = signerId.HasValue ? Get(signerId.Value) : null;

            if (clientSignersAllowedDocumentTypes.Any())
            {
                if (clientSigner != null)
                {
                    if (client.LegalForm.IsIndividual)
                        throw new PawnshopApplicationException(
                            $"С правовой формой {client.LegalForm.Name} для клиента {client.FullName} нельзя завести подписанта");

                    if (clientSigner.SignerDocument.DocumentType.Code == Constants.POWER_OF_ATTORNEY &&
                        clientSigner.SignerDocument.DateExpire < contractDate)
                        throw new PawnshopApplicationException(
                            $"Истек срок действия доверенности у клиента {clientSigner.Signer.FullName}");

                    clientSigner.Signer = _clientRepository.Get(clientSigner.SignerId);
                    _clientModelValidateService.ValidateClientModel(clientSigner.Signer);
                }

                if ((!signerId.HasValue || clientSigner == null) && clientSignersAllowedDocumentTypes.Any(t => t.IsMandatory))
                    throw new PawnshopApplicationException(
                        $"С правовой формой {client.LegalForm.Name} для клиента {client.FullName} обязательно наличие подписанта");
            }

            return clientSigner;
        }

        public List<ClientSignersAllowedDocumentType> GetClientSignersAllowedDocumentTypes(int legalFormId)
        {
            return _clientSignersAllowedDocumentTypeService.List(new ListQuery() { Page = null }, new { CompanyLegalFormId = legalFormId }).List;
        }

        public List<ClientSigner> GetList(int companyId)
        {
            if (companyId <= 0)
                throw new ArgumentNullException(nameof(companyId));

            _clientService.Get(companyId);

            List<ClientSigner> clientSigners = _repository.List(new ListQuery(), new { CompanyId = companyId });

            return clientSigners;
        }

        private void ValidateCompanySigner(int companyId, List<ClientSigner> companySigners)
        {
            if (companySigners == null)
                throw new ArgumentNullException(nameof(companyId));

            Client client = _clientService.Get(companyId);
            var errors = new HashSet<string>();

            if (companySigners.Where(e => e.Id != 0).GroupBy(e => e.Id).Any(e => e.Count() > 1))
                errors.Add("Обнаружены несколько записей с одним Id, обратитесь в тех. поддержку");

            Dictionary<int, DomainValue> signerPositionDomainValuesDict = _domainService.GetDomainValues(Constants.SIGNER_POSITIONS_DOMAIN_VALUE).ToDictionary(dv => dv.Id, dv => dv);

            // проверим на существования клиента для подписанта
            companySigners.ForEach(e => _clientService.Get(e.SignerId));

            // проверим на null значения в поле SignerId
            bool nullSignerIdExists = companySigners.Any(e => e.SignerId == 0);
            if (nullSignerIdExists)
                errors.Add($"Не все подписанты имеют заполненный {nameof(ClientSigner.SignerId)}");

            // проверим на null значения в поле SignerPositionId
            bool nullSignerPositionIdExists = companySigners.Any(e => e.SignerPositionId == 0);
            if (nullSignerPositionIdExists)
                errors.Add($"Не все подписанты имеют заполненный {nameof(ClientSigner.SignerPositionId)}");

            // проверим на null значения в поле DocumentId
            bool nullDocumentIdExists = companySigners.Any(e => e.DocumentId == 0);
            if (nullDocumentIdExists)
                errors.Add($"Не все подписанты имеют заполненный {nameof(ClientSigner.DocumentId)}");

            // проверим чтобы значения SignerPositionId были валидными
            HashSet<int> uniqueCompanySignersIds = companySigners.Select(e => e.SignerPositionId).ToHashSet();
            if (!uniqueCompanySignersIds.IsSubsetOf(signerPositionDomainValuesDict.Keys))
                errors.Add($"Не все подписанты имеют правильный {nameof(ClientSigner.SignerPositionId)}");

            bool nullDocumentExists = companySigners.Any(e => e.SignerDocument == null);
            if (nullDocumentExists)
                errors.Add($"Не все подписанты имеют заполненный {nameof(ClientSigner.SignerDocument)}");

            if (!nullDocumentExists && companySigners.Any())
                foreach (var companySigner in companySigners)
                {
                    var document = companySigner.SignerDocument;
                    var sameSigners = companySigners.Where(c => c.SignerId == companySigner.SignerId && c.Id != companySigner.Id);

                    if (sameSigners.Any())
                    {
                        bool checkForType(ClientSigner c) => c.SignerDocument.TypeId == document.TypeId;
                        var sameTypes = sameSigners.Where(checkForType);

                        switch (document.DocumentType.Code)
                        {
                            case Constants.CHARTER:
                                {
                                    if (sameTypes.Count() > 1)
                                        errors.Add($"Один и тот же подписант не может иметь устав дважды");
                                    break;
                                }
                            case Constants.ORDER:
                            case Constants.POWER_OF_ATTORNEY:
                                {
                                    bool checkForNumber(ClientSigner c) => c.SignerDocument.Number == document.Number;
                                    bool checkForDate(ClientSigner c) => c.SignerDocument.DateExpire == document.DateExpire && c.SignerDocument.Date == document.Date;

                                    var sameNumbers = sameTypes.Where(checkForNumber);

                                    if (sameNumbers.Count() > 1 && document.DocumentType.Code == Constants.ORDER)
                                        errors.Add($"Один и тот же подписант не может иметь приказ с одинаковым номером дважды");
                                    else if (document.DocumentType.Code == Constants.POWER_OF_ATTORNEY && sameNumbers.Any(checkForDate))
                                        errors.Add($"Один и тот же подписант не может иметь доверенность с одинаковым номером и сроком дважды");
                                    break;
                                }
                        }
                    }
                }

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }

        private bool CheckCompanySignersChangedFromDBModel(ClientSigner companySigner, ClientSigner companySignerFromDB)
        {
            if (companySigner == null)
                throw new ArgumentNullException(nameof(companySigner));

            if (companySignerFromDB == null)
                throw new ArgumentNullException(nameof(companySignerFromDB));

            if (companySigner.Id != companySignerFromDB.Id)
                throw new InvalidOperationException($"{nameof(companySigner)}.{nameof(companySigner.Id)} должен быть равен {nameof(companySignerFromDB)}.{nameof(companySignerFromDB.Id)}");

            return companySignerFromDB.CompanyId != companySigner.CompanyId ||
                   companySignerFromDB.SignerId != companySigner.SignerId ||
                   companySignerFromDB.DocumentId != companySigner.DocumentId ||
                   companySignerFromDB.SignerPositionId != companySigner.SignerPositionId;
        }

        public List<ClientSigner> Save(int companyId, List<ClientSigner> companySignersRequest)
        {
            if (companyId <= 0)
                throw new ArgumentNullException(nameof(companyId));

            Dictionary<int, ClientSigner> companySignersFromDB = _repository.List(new ListQuery(), new { CompanyId = companyId }).ToDictionary(c => c.Id, c => c);
            ValidateCompanySigner(companyId, companySignersRequest);

            var syncedCompanySigners = new List<ClientSigner>();

            foreach (var companySigner in companySignersRequest)
            {
                ClientSigner companySignerFromDB = null;
                if (!companySignersFromDB.TryGetValue(companySigner.Id, out companySignerFromDB))
                {
                    companySignerFromDB = new ClientSigner
                    {
                        CompanyId = companyId,
                        Signer = companySigner.Signer,
                        SignerId = companySigner.SignerId,
                        SignerPositionId = companySigner.SignerPositionId,
                        DocumentId = companySigner.DocumentId,
                        CreateDate = DateTime.Now,
                        AuthorId = _sessionContext.UserId
                    };
                }
                else
                {
                    if (CheckCompanySignersChangedFromDBModel(companySigner, companySignerFromDB))
                    {
                        companySignerFromDB.SignerId = companySigner.SignerId;
                        companySignerFromDB.SignerPositionId = companySigner.SignerPositionId;
                        companySignerFromDB.DocumentId = companySigner.DocumentId;
                    }

                    companySignersFromDB.Remove(companySigner.Id);
                }
                syncedCompanySigners.Add(companySignerFromDB);
            }

            if (companySignersFromDB.Count > 0 || syncedCompanySigners.Count > 0)
                using (var transaction = _repository.BeginTransaction())
                {
                    foreach ((int key, var companySigner) in companySignersFromDB)
                    {
                        _repository.Delete(key);
                    }

                    foreach (var companySigner in syncedCompanySigners)
                    {
                        if (companySigner.Id != default)
                        {
                            _repository.Update(companySigner);
                            //_clientEconomicActivityRepository.LogChanges(clientEconomicActivity, _sessionContext.UserId, true);
                        }
                        else
                        {
                            _repository.Insert(companySigner);
                            //_clientEconomicActivityRepository.LogChanges(clientEconomicActivity, _sessionContext.UserId);
                        }
                    }

                    transaction.Commit();
                }

            return syncedCompanySigners;
        }
    }
}