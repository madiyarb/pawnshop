using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Integrations.UKassa;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Services.Remittances
{
    public class RemittanceService : IRemittanceService
    {
        private readonly RemittanceRepository _remittanceRepository;
        private readonly GroupRepository _groupRepository;
        private readonly UserRepository _userRepository;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly ICashOrderService _cashOrderService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IEventLog _eventLog;
        private readonly IUKassaService _uKassaService;

        public RemittanceService(RemittanceRepository remittanceRepository, ICashOrderService cashOrderService, 
            GroupRepository groupRepository, IBusinessOperationService businessOperationService,
            UserRepository userRepository, InnerNotificationRepository innerNotificationRepository,
            PayTypeRepository payTypeRepository, IEventLog eventLog,
            IUKassaService uKassaService)
        {
            _remittanceRepository = remittanceRepository;
            _cashOrderService = cashOrderService;
            _groupRepository = groupRepository;
            _businessOperationService = businessOperationService;
            _userRepository = userRepository;
            _payTypeRepository = payTypeRepository;
            _innerNotificationRepository = innerNotificationRepository;
            _eventLog = eventLog;
            _uKassaService = uKassaService;
        }

        public async Task<Remittance> GetAsync(int remittanceId)
        {
            return await _remittanceRepository.GetAsync(remittanceId);
        }

        public Remittance Update(int id, int branchIdTo, decimal sum, string note, int authorId, int? branchId = null)
        {
            if (sum <= 0)
                throw new ArgumentOutOfRangeException("argument cannot be less or equal than zero", nameof(sum));

            if (sum > int.MaxValue)
                throw new ArgumentOutOfRangeException($"argument cannot be less or equal than int.Max {int.MaxValue}", nameof(sum));

            Remittance remittance = _remittanceRepository.Get(id);
            if (remittance == null)
                throw new PawnshopApplicationException($"Перевод {id} не найден");

            if (branchId.HasValue && remittance.SendBranchId != branchId.Value)
                throw new PawnshopApplicationException($"Перевод {id}, нельзя редактировать перевод через филиал не являющимся его создателем");

            bool isAccepted = remittance.Status == RemittanceStatusType.Received || remittance.SendOrderId.HasValue || remittance.ReceiveOrderId.HasValue
                || remittance.ReceiveDate.HasValue || remittance.ReceiveUserId.HasValue;
            if (isAccepted)
                throw new PawnshopApplicationException($"Перевод {id} уже является принятым, нельзя редактировать");

            Group toBranch = _groupRepository.Get(branchIdTo);
            if (toBranch == null)
                throw new PawnshopApplicationException($"Филиал {branchIdTo} не найден");

            Group fromBranch = _groupRepository.Get(remittance.SendBranchId);
            if (fromBranch == null)
                throw new PawnshopApplicationException($"Филиал {remittance.SendBranchId} не найден");

            if (toBranch.Id == fromBranch.Id)
                throw new PawnshopApplicationException($"Нельзя указывать один и тот же филиал в отправляющем и принимающем филиалах");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            remittance.ReceiveBranchId = toBranch.Id;
            remittance.SendCost = (int)sum;
            remittance.SendDate = DateTime.Now;
            remittance.SendUserId = author.Id;
            remittance.Note = note;
            using (IDbTransaction transaction = _remittanceRepository.BeginTransaction())
            {
                InnerNotification notification = new InnerNotification()
                {
                    Message = $"Ранее отправленный перевод({remittance.Id}) из филиала {fromBranch.DisplayName} был изменен: сумма {sum}, примечание: {note}",
                    CreateDate = DateTime.Now,
                    CreatedBy = author.Id,
                    ReceiveBranchId = toBranch.Id,
                    EntityType = EntityType.Remittance,
                    EntityId = remittance.Id,
                    Status = InnerNotificationStatus.Sent
                };
                _innerNotificationRepository.Insert(notification);
                remittance.InnerNotificationId = notification.Id;
                _remittanceRepository.Update(remittance);
                var requestData = new { id, branchIdTo, sum, note, authorId, branchId };
                string requestDataJson = JsonConvert.SerializeObject(requestData);
                string remittanceJson = JsonConvert.SerializeObject(remittance);
                _eventLog.Log(Data.Models.Audit.EventCode.RemittanceSaved, Data.Models.Audit.EventStatus.Success, EntityType.Remittance, remittance.Id, requestData: requestDataJson, responseData: remittanceJson);
                transaction.Commit();
                return remittance;
            }
        }

        public Remittance Register(int branchIdFrom, int branchIdTo, decimal sum, string note, int authorId)
        {   
            if (sum <= 0)
                throw new ArgumentOutOfRangeException("argument cannot be less or equal than zero", nameof(sum));

            if (sum > int.MaxValue)
                throw new ArgumentOutOfRangeException($"argument cannot be less or equal than int.Max {int.MaxValue}", nameof(sum));

            Group fromBranch = _groupRepository.Get(branchIdFrom);
            if (fromBranch == null)
                throw new PawnshopApplicationException($"Филиал {branchIdFrom} не найден");

            Group toBranch = _groupRepository.Get(branchIdTo);
            if (toBranch == null)
                throw new PawnshopApplicationException($"Филиал {branchIdTo} не найден");

            if (toBranch.Id == fromBranch.Id)
                throw new PawnshopApplicationException($"Нельзя указывать один и тот же филиал в отправляющем и принимающем филиалах");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            using (IDbTransaction transaction = _remittanceRepository.BeginTransaction())
            {
                var remittance = new Remittance
                {
                    ReceiveBranchId = toBranch.Id,
                    SendBranchId = fromBranch.Id,
                    CreateDate = DateTime.Now,
                    SendCost = (int)sum,
                    Note = note,
                    Status = RemittanceStatusType.Sent,
                    SendDate = DateTime.Now,
                    SendUserId = author.Id
                };

                _remittanceRepository.Insert(remittance);
                InnerNotification notification = new InnerNotification()
                {
                    Message = $"На ваш филиал пришёл перевод на сумму {sum} {(string.IsNullOrWhiteSpace(note) ? string.Empty : $" с примечанием: {note}")}",
                    CreateDate = DateTime.Now,
                    CreatedBy = author.Id,
                    ReceiveBranchId = toBranch.Id,
                    EntityType = EntityType.Remittance,
                    EntityId = remittance.Id,
                    Status = InnerNotificationStatus.Sent
                };
                _innerNotificationRepository.Insert(notification);
                remittance.InnerNotificationId = notification.Id;
                _remittanceRepository.Update(remittance);
                var requestData = new { branchIdFrom, branchIdTo, sum, note, authorId };
                string requestDataJson = JsonConvert.SerializeObject(requestData);
                string remittanceJson = JsonConvert.SerializeObject(remittance);
                _eventLog.Log(Data.Models.Audit.EventCode.RemittanceSaved, Data.Models.Audit.EventStatus.Success, EntityType.Remittance, remittance.Id, requestData: requestDataJson, responseData: remittanceJson);
                transaction.Commit();
                return remittance;
            }
        }
        
        public async Task<Remittance> RegisterAsync(int branchIdFrom, int branchIdTo, decimal sum, string note, int authorId)
        {   
            if (sum <= 0)
                throw new ArgumentOutOfRangeException("аргумент не может быть меньше или равен нулю", nameof(sum));

            if (sum > int.MaxValue)
                throw new ArgumentOutOfRangeException($"аргумент не может быть меньше или равен int.Max: {int.MaxValue}", nameof(sum));

            Group fromBranch = await GetBranch(branchIdFrom);
            Group toBranch = await GetBranch(branchIdTo);

            if (toBranch.Id == fromBranch.Id)
            {
                throw new PawnshopApplicationException(
                "Нельзя указывать один и тот же филиал в отправляющем и принимающем филиалах");
            }

            User author = await GetUser(authorId);
            var remittance = new Remittance
            {
                ReceiveBranchId = toBranch.Id,
                SendBranchId = fromBranch.Id,
                CreateDate = DateTime.Now,
                SendCost = (int)sum,
                Note = note,
                Status = RemittanceStatusType.Sent,
                SendDate = DateTime.Now,
                SendUserId = author.Id
            };
            
            var notification = new InnerNotification
            {
                Message = $"На ваш филиал пришёл перевод на сумму {sum} {(string.IsNullOrWhiteSpace(note) ? string.Empty : $" с примечанием: {note}")}",
                CreateDate = DateTime.Now,
                CreatedBy = author.Id,
                ReceiveBranchId = toBranch.Id,
                EntityType = EntityType.Remittance,
                EntityId = remittance.Id,
                Status = InnerNotificationStatus.Sent
            };
            
            var requestData = new { branchIdFrom, branchIdTo, sum, note, authorId };
            string requestDataJson = JsonConvert.SerializeObject(requestData);
            string remittanceJson = JsonConvert.SerializeObject(remittance);

            using (IDbTransaction transaction = _remittanceRepository.BeginTransaction())
            {
                await _remittanceRepository.InsertAsync(remittance);
                await _innerNotificationRepository.InsertAsync(notification);
                
                remittance.InnerNotificationId = notification.Id;
                await _remittanceRepository.UpdateAsync(remittance);
                
                await _eventLog.LogAsync(
                    Data.Models.Audit.EventCode.RemittanceSaved,
                    Data.Models.Audit.EventStatus.Success,
                    EntityType.Remittance,
                    remittance.Id,
                    requestData: requestDataJson,
                    responseData: remittanceJson);
                
                transaction.Commit();
                return remittance;
            }
        }

        public Remittance Accept(int id, int authorId, int? branchId = null)
        {
            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            Remittance remittance = _remittanceRepository.Get(id);
            if (remittance == null)
                throw new PawnshopApplicationException($"Перевод {id} не найден");

            if (branchId.HasValue && remittance.ReceiveBranchId != branchId.Value)
                throw new PawnshopApplicationException($"Перевод {id}, нельзя подтвердить перевод через филиал не являющимся получателем");

            if (remittance.SendDate.Date < DateTime.Today)
                throw new PawnshopApplicationException($"Дата перевода({id}) не равна текущему дню, создайте новый перевод");

            bool isAccepted = remittance.Status == RemittanceStatusType.Received || remittance.SendOrderId.HasValue || remittance.ReceiveOrderId.HasValue
                || remittance.ReceiveDate.HasValue || remittance.ReceiveUserId.HasValue;
            if (isAccepted)
                throw new PawnshopApplicationException("Перевод {id} уже является подтвержденным, нельзя подтвердить");

            PayType defaultPayType = _payTypeRepository.Find(new { IsDefault = true });
            if (defaultPayType == null)
                throw new PawnshopApplicationException("Тип платежа по умолчанию не найден");

            using (IDbTransaction transaction = _remittanceRepository.BeginTransaction())
            {
                remittance.ReceiveDate = DateTime.Now;
                remittance.ReceiveUserId = author.Id;
                remittance.Status = RemittanceStatusType.Received;
                var outAmountsDict = new Dictionary<AmountType, decimal> {
                    { AmountType.Remittance, remittance.SendCost }
                };

                var inAmountsDict = new Dictionary<AmountType, decimal> {
                    { AmountType.Remittance, remittance.SendCost }
                };

                List<(CashOrder, List<AccountRecord>)> outResult = _businessOperationService.Register(remittance.SendDate, Constants.BO_REMITTANCE_OUT, remittance.SendBranchId, remittance.SendUserId, outAmountsDict, defaultPayType.Id, orderUserId: remittance.SendUserId, note: remittance.Note, remittanceBranchId: remittance.ReceiveBranchId);
                if (outResult == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(outResult)} не будет null");

                if (outResult.Count == 0)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(outResult)} будет содержать хоть какие то элементы");

                if (outResult.Count > 1)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(outResult)} будет содержать только один элемент");

                (CashOrder outCashOrder, List<AccountRecord> _) = outResult.Single();
                if (outCashOrder == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(outCashOrder)} не будет null");

                List<(CashOrder, List<AccountRecord>)> inResult = _businessOperationService.Register(remittance.ReceiveDate.Value, Constants.BO_REMITTANCE_IN, remittance.ReceiveBranchId, authorId, inAmountsDict, defaultPayType.Id, orderUserId: authorId, note: remittance.Note, remittanceBranchId: remittance.SendBranchId);
                if (inResult == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(inResult)} не будет null");

                if (inResult.Count == 0)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(inResult)} будет содержать хоть какие то элементы");

                if (inResult.Count > 1)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(inResult)} будет содержать только один элемент");

                (CashOrder inCashOrder, List<AccountRecord> _) = inResult.Single();
                if (inCashOrder == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(inCashOrder)} не будет null");

                remittance.SendOrderId = outCashOrder.Id;
                remittance.ReceiveOrderId = inCashOrder.Id;
                _remittanceRepository.Update(remittance);
                var requestData = new { id, authorId, branchId };
                string requestDataJson = JsonConvert.SerializeObject(requestData);
                string remittanceJson = JsonConvert.SerializeObject(remittance);
                _eventLog.Log(Data.Models.Audit.EventCode.RemittanceReceived, Data.Models.Audit.EventStatus.Success, EntityType.Remittance, remittance.Id, requestData: requestDataJson, responseData: remittanceJson);
                transaction.Commit();
                var orderIds = outResult.Select(x => x.Item1.Id).ToList();
                orderIds.AddRange(inResult.Select(x => x.Item1.Id).ToList());
                _uKassaService.FinishRequests(orderIds);
                return remittance;
            }
        }

        public void Delete(int id, int? branchId = null)
        {
            Remittance remittance = _remittanceRepository.Get(id);
            if (remittance == null)
                throw new PawnshopApplicationException($"Денежный перевод между филиалами под номером {id} не найден");

            if (branchId.HasValue && remittance.SendBranchId != branchId.Value)
                throw new PawnshopApplicationException($"Перевод {id}, нельзя удалить перевод через филиал не являющимся его создателем");

            remittance.SendOrderId = null;
            remittance.ReceiveOrderId = null;
            remittance.ReceiveUserId = null;
            remittance.ReceiveDate = null;

            bool isAccepted = remittance.Status == RemittanceStatusType.Received || remittance.SendOrderId.HasValue || remittance.ReceiveOrderId.HasValue
                || remittance.ReceiveDate.HasValue || remittance.ReceiveUserId.HasValue;
            if (isAccepted)
                throw new PawnshopApplicationException($"Денежный перевод между филиалами под номером {id} является подтвержденным, невозможно удалить");

            _remittanceRepository.Delete(remittance.Id);
            var requestData = new { id };
            string requestDataJson = JsonConvert.SerializeObject(requestData);
            _eventLog.Log(Data.Models.Audit.EventCode.RemittanceDeleted, Data.Models.Audit.EventStatus.Success, EntityType.Remittance, remittance.Id, requestData: requestDataJson);
        }

        public async Task CancelAcceptAsync(int id, int authorId, int? branchId = null)
        {
            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            Remittance remittance = _remittanceRepository.Get(id);
            if (remittance == null)
                throw new PawnshopApplicationException($"Денежный перевод между филиалами под номером {id} не найден");

            if (branchId.HasValue && remittance.ReceiveBranchId != branchId.Value)
                throw new PawnshopApplicationException($"Перевод {id}, нельзя подтвердить перевод через филиал не являющимся ее получателем");

            bool isAccepted = remittance.Status == RemittanceStatusType.Received || remittance.SendOrderId.HasValue || remittance.ReceiveOrderId.HasValue
                || remittance.ReceiveDate.HasValue || remittance.ReceiveUserId.HasValue;
            if (!isAccepted)
                throw new PawnshopApplicationException($"Перевод {id} является неподтвержденным, нельзя отменить подтверждение");

            if (!remittance.SendOrderId.HasValue)
                throw new PawnshopApplicationException($"Денежный перевод между филиалами под номером {id} не содержит ордер отправки");

            if (!remittance.SendOrderId.HasValue)
                throw new PawnshopApplicationException($"Денежный перевод между филиалами под номером {id} не содержит ордер Получения");

            Group sendBranch = _groupRepository.Get(remittance.SendBranchId);
            if (sendBranch == null)
                throw new PawnshopApplicationException($"Филиал {remittance.SendBranchId} не найден");

            Group receiveBranch = _groupRepository.Get(remittance.ReceiveBranchId);
            if (receiveBranch == null)
                throw new PawnshopApplicationException($"Филиал {remittance.ReceiveBranchId} не найден");

            using (IDbTransaction transaction = _remittanceRepository.BeginTransaction())
            {
                CashOrder receiveOrder = await _cashOrderService.GetAsync(remittance.ReceiveOrderId.Value);
                if (receiveOrder == null)
                    throw new PawnshopApplicationException($"Приходный кассовый ордер не найден у денежного перевода между филилалами под номером {remittance.Id}");

                var canceledReceiveOrder = _cashOrderService.Cancel(receiveOrder, author.Id, receiveBranch);
                CashOrder sendOrder = await _cashOrderService.GetAsync(remittance.SendOrderId.Value);
                if (sendOrder == null)
                    throw new PawnshopApplicationException($"Расходный кассовый ордер не найден у денежного перевода между филилалами под номером {remittance.Id}");

                var canceledSendOrder = _cashOrderService.Cancel(sendOrder, author.Id, sendBranch);
                remittance.SendOrderId = null;
                remittance.ReceiveOrderId = null;
                remittance.ReceiveUserId = null;
                remittance.ReceiveDate = null;
                remittance.Status = RemittanceStatusType.Sent;
                _remittanceRepository.Update(remittance);
                var requestData = new { id, authorId, branchId };
                string requestDataJson = JsonConvert.SerializeObject(requestData);
                string remittanceJson = JsonConvert.SerializeObject(remittance);
                _eventLog.Log(Data.Models.Audit.EventCode.RemittanceReceiveCanceled, Data.Models.Audit.EventStatus.Success, EntityType.Remittance, remittance.Id, requestData: requestDataJson, responseData: remittanceJson);
                transaction.Commit();
                var orderIds = new List<int>() { canceledReceiveOrder.Id, canceledSendOrder.Id };
                _uKassaService.FinishRequests(orderIds);
            }
        }

        private async Task<Group> GetBranch(int branchId)
        {
            Group branch = await _groupRepository.GetAsync(branchId);
            
            if (branch == null)
            {
                throw new PawnshopApplicationException($"Филиал с Id: {branchId} не найден");
            }

            return branch;
        }

        private async Task<User> GetUser(int userId)
        {
            User user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new PawnshopApplicationException($"Пользователь с Id:{userId} не найден");
            }

            return user;
        }
    }
}