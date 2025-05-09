using MediatR;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Services.HardCollection.Service.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Services.HardCollection.Service.Impl
{
    public class HardCollectionService : IHardCollectionService
    {
        private readonly IMediator _mediator;
        private readonly HCContractStatusRepository _repository;

        public HardCollectionService(IMediator mediator, HCContractStatusRepository repository)
        {
            _mediator = mediator;
            _repository = repository;
        }

        public async Task HCollectionJobSender()
        {

            int counterNew = 0;
            int counterOld = 0;
            int counterNotFishNotMeal = 0;
            var contractList = (await _repository.ListAllAsync()).ToList();
            var hcList = (await _repository.ListAsync()).ToList();
            int counterList = contractList.Count;

            LogNotification lognotification = new LogNotification()
            {
                EventLogNotification = null,
                FileLogNotification = null,
                TelegaLogNotification = new TelegramLogNotification()
                {
                    LogNotificationText = @$"Начала работы джоба HardCollection
Количество договоров для обработки={counterList}"
                }
            };

            HardCollectionNotification notification = new HardCollectionNotification()
            {
                HistoryNotification = null,
                LogNotification = lognotification
            };
            await _mediator.Publish(notification);

            foreach (var item in contractList)
            {
                if (item.StatusCode != Constants.HARDCOLLECTION_STATUS &&
                    item.StatusCode != Constants.LEGALHARDCOLLECTION_STATUS)
                    continue;

                var hcRecord = hcList.FirstOrDefault(x => x.ContractId == item.ContractId);

                if (hcRecord == null || !hcRecord.IsActive)
                {
                    var newHcModel = new HCContractStatus
                    {
                        ContractId = item.ContractId,
                        StageId = (int)HardCollectionStageEnum.ComeToWork,
                        IsActive = true
                    };

                    if (hcRecord == null)
                    {
                        _repository.Insert(newHcModel);
                    }
                    else if (!hcRecord.IsActive)
                    {
                        newHcModel.Id = hcRecord.Id;
                        _repository.Update(newHcModel);
                    }

                    var contractData = await _mediator.Send(new GetContractDataQuery() { ContractId = item.ContractId, IsJobWorking = true });
                    if (contractData == null || contractData.ContractCar == null || contractData.Contract == null || contractData.ContractClient == null)
                        continue;

                    if (await _mediator.Send(new SendContractDataCommand() { ContractId = item.ContractId, ContractData = contractData, IsJobWorking = true }))
                        counterNew++;
                }
                else if (hcRecord.IsActive)
                {
                    var contractDataOnly = await _mediator.Send(new GetContractOnlyQuery() { ContractId = item.ContractId, IsJobWorking = true });
                    if (contractDataOnly == null)
                        continue;

                    if (await _mediator.Send(new SendContractOnlyCommand() { ContractId = item.ContractId, ContractDataOnly = contractDataOnly, IsJobWorking = true }))
                        counterOld++;
                }
                else
                {
                    counterNotFishNotMeal++;
                }
            }

            lognotification = new LogNotification()
            {
                EventLogNotification = null,
                FileLogNotification = null,
                TelegaLogNotification = new TelegramLogNotification()
                {
                    LogNotificationText = @$"Конец работы джоба HardCollection
Количество новых договоров={counterNew}
Количество договоров для обновления={counterOld}
Количество не понятных договоров={counterNotFishNotMeal}"
                }
            };

            notification = new HardCollectionNotification()
            {
                HistoryNotification = null,
                LogNotification = lognotification
            };
            await _mediator.Publish(notification);
        }
    }
}
