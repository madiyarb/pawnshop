using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Kdn;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Integrations.Fcb;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Services.Storage;
using System;
using System.Threading.Tasks;

namespace Pawnshop.Services.Gamblers
{
    public class FCBChecksService : IFCBChecksService
    {
        private readonly IClientService _clientService;
        private readonly IFcb4Kdn _fcb4Kdn;
        private readonly IStorage _storage;
        private readonly ClientsBlackListRepository _clientsBlackListRepository;
        private readonly BlackListReasonRepository _blackListReasonRepository;
        private readonly ISessionContext _sessionContext;

        public FCBChecksService(
            IClientService clientService,
            IFcb4Kdn fcb4Kdn,
            IStorage storage,
            ISessionContext sessionContext,
            BlackListReasonRepository blackListReasonRepository,
            ClientsBlackListRepository clientsBlackListRepository)
        {
            _clientService = clientService;
            _fcb4Kdn = fcb4Kdn;
            _storage = storage;
            _blackListReasonRepository = blackListReasonRepository;
            _clientsBlackListRepository = clientsBlackListRepository;
            _sessionContext = sessionContext;
        }

        public async Task<FCBChecksResult> GamblerFeatureCheck(int clientId, User author)
        {
            var client = _clientService.GetOnlyClient(clientId);
            var request = new FcbReportRequest()
            {
                Author = author.Fullname,
                AuthorId = author.Id,
                ClientId = client.Id,
                Creditinfoid = 0,
                DocumentType = 14,
                IIN = client.IdentityNumber,
                OrganizationId = 1,
                ReportType = FCBReportTypeCode.IndividualStandard
            };
            var report = await _fcb4Kdn.GetReport(request);

            if (_fcb4Kdn.ValidateReportResponse(report))
                throw new PawnshopApplicationException(report?.AvailableReportsErrorResponse?.Errmessage?.Value);

            if (report != null && report.XmlLink != null)
            {
                var FCBChecksResult = await _fcb4Kdn
                        .FCBChecks(
                            await _storage.Load(
                                report.XmlLink,
                                (ContainerName)Enum.Parse(typeof(ContainerName),
                                report.FolderName)));

                if (FCBChecksResult.IsGumbler)
                {
                    await SaveClientToBlackList(clientId, author);
                }

                return FCBChecksResult;
            }

            return new FCBChecksResult()
            {
                IsGumbler = false,
                IsStopCredit = false,
            };
        }

        private async Task SaveClientToBlackList(int clientId, User author)
        {
            var reasonId = _blackListReasonRepository.Find(new { Name = "Азартный игрок" }).Id;
            var clientInBlackList = _clientsBlackListRepository.Find(new
            {
                ClientId = clientId,
                ReasonId = reasonId
            });

            if (clientInBlackList is null)
            {
                clientInBlackList = new ClientsBlackList()
                {
                    ClientId = clientId,
                    ReasonId = reasonId,
                    AddReason = "Автоматическое внесение в список ЧС системой при проверке на Лудоманства",
                    AddedBy = author.Id,
                    AddedAt = DateTime.Now
                };

                _clientsBlackListRepository.Insert(clientInBlackList);
                _clientsBlackListRepository.LogChanges(clientInBlackList, _sessionContext.UserId);
            }
        }
    }
}
