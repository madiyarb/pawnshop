using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System.Text;
using System;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Newtonsoft.Json;

namespace Pawnshop.Services.HardCollection.Query
{
    public class GetHistoryListQueryHandler : IRequestHandler<GetHistoryListQuery, List<HCActionHistoryVM>>
    {
        private readonly IMediator _mediator;
        private readonly HCActionHistoryRepository _historyRepository;
        private readonly ClientAddressRepository _clientAddressRepository;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly ClientAdditionalContactRepository _additionalContactRepository;
        private readonly FileRepository _fileRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ContractExpenseRepository _expenseRepository;
        private readonly HCGeoDataRepository _geoDataRepository;

        public GetHistoryListQueryHandler(IMediator mediator, 
            HCActionHistoryRepository historyRepository,
            ClientAddressRepository clientAddressRepository,
            ClientContactRepository clientContactRepository,
            ClientAdditionalContactRepository additionalContactRepository,
            FileRepository fileRepository,
            ClientRepository clientRepository,
            ContractExpenseRepository expenseRepository,
            HCGeoDataRepository geoDataRepository)
        {
            _mediator = mediator;
            _historyRepository = historyRepository;
            _clientAddressRepository = clientAddressRepository;
            _clientContactRepository = clientContactRepository;
            _additionalContactRepository = additionalContactRepository;
            _fileRepository = fileRepository;
            _clientRepository = clientRepository;
            _expenseRepository = expenseRepository;
            _geoDataRepository = geoDataRepository;
        }

        public async Task<List<HCActionHistoryVM>> Handle(GetHistoryListQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = query.ContractId });

                var historyList = _historyRepository.GetByContractId(query.ContractId);

                var historyVmList = new List<HCActionHistoryVM>();

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(@"MethodName: GetHistoryListQuery
IsSucceeded=True");
                for (int i = 0; i < historyList.Count; i++)
                {
                    var historyVm = new HCActionHistoryVM();
                    historyVm.Id = historyList[i].Id;
                    historyVm.HCContractStatusId = historyList[i].HCContractStatusId;
                    historyVm.ActionName = historyList[i].ActionName;
                    historyVm.ActionId = historyList[i].ActionId;
                    if(historyList[i].Value.HasValue)
                    {
                        historyVm.Value = await GetObjectValue(historyList[i].Value.Value, (HardCollectionActionTypeEnum)historyList[i].ActionId);
                    }
                    historyVm.GeoData = await GetGeoObject(historyList[i].Id);
                    historyVm.AuthorId = historyList[i].AuthorId;
                    historyVm.Comment = historyList[i].Comment;
                    historyVm.CreateDate = historyList[i].CreateDate;

                    stringBuilder.AppendLine(historyVm.ToString());
                    stringBuilder.AppendLine();
                    historyVmList.Add(historyVm);
                }

                var notification = new HardCollectionNotification()
                {
                    LogNotification = query.GetLogNotifications($"Получили с БД данные по контракту с ContractId={query.ContractId}", null, Data.Models.Audit.EventStatus.Success, true, false)
                };

                notification.LogNotification.TelegaLogNotification = new TelegramLogNotification() { LogNotificationText = stringBuilder.ToString() };

                await _mediator.Publish(notification);

                return historyVmList;
            }
            catch(Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = query.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed, true, false)
                };
                await _mediator.Publish(notification);

                throw;
            }
        }

        private async Task<string> GetObjectValue(int value, HardCollectionActionTypeEnum action)
        {
            string valueString;
            switch(action) 
            {
                case HardCollectionActionTypeEnum.AddAddress:
                    valueString = JsonConvert.SerializeObject(await _clientAddressRepository.GetAsync(value));
                    break;
                case HardCollectionActionTypeEnum.AddActualContact:
                    valueString = JsonConvert.SerializeObject(await _clientContactRepository.GetAsync(value));
                    break;
                case HardCollectionActionTypeEnum.AddContact:
                    valueString = JsonConvert.SerializeObject(await _additionalContactRepository.GetAsync(value));
                    break;
                case HardCollectionActionTypeEnum.SaveAcceptanceCertificate:
                    valueString = JsonConvert.SerializeObject(await _fileRepository.GetAsync(value));
                    break;
                case HardCollectionActionTypeEnum.AddWitness:
                    valueString = JsonConvert.SerializeObject(await _clientRepository.GetOnlyClientAsync(value));
                    break;
                case HardCollectionActionTypeEnum.AddExpence:
                    valueString = JsonConvert.SerializeObject(await _expenseRepository.GetAsync(value));
                    break;
                default: valueString = null;
                    break;
            }

            return valueString;
        }

        private async Task<string> GetGeoObject(int historyId)
        {
            var value = await _geoDataRepository.GetByHistoryIdAsync(historyId);
            if(value is null)
                return null;

            return JsonConvert.SerializeObject((HCGeoDataVM)value);
        }
    }
}
