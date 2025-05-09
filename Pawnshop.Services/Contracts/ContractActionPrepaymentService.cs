using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Processing;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Models.Calculation;
using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Text;
using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Services.Contracts
{
    public class ContractActionPrepaymentService : IContractActionPrepaymentService
    {
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractDutyService _contractDutyService;
        private readonly ContractRepository _contractRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly GroupRepository _groupRepository;
        private readonly UserRepository _userRepository;
        private readonly IContractService _contractService;
        private readonly IAccountService _accountService;
        private readonly IContractActionService _contractActionService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IEventLog _eventLog;

        public ContractActionPrepaymentService(
            IContractActionOperationService contractActionOperationService,
            ContractRepository contractRepository,
            PayTypeRepository payTypeRepository,
            GroupRepository groupRepository,
            IContractDutyService contractDutyService,
            IContractService contractService,
            UserRepository userRepository,
            IAccountService accountService,
            IContractActionService contractActionService,
            IBusinessOperationService businessOperationService,
            IEventLog eventLog)
        {
            _contractRepository = contractRepository;
            _contractActionOperationService = contractActionOperationService;
            _payTypeRepository = payTypeRepository;
            _groupRepository = groupRepository;
            _contractDutyService = contractDutyService;
            _contractService = contractService;
            _userRepository = userRepository;
            _accountService = accountService;
            _contractActionService = contractActionService;
            _businessOperationService = businessOperationService;
            _eventLog = eventLog;
        }

        private const string BAD_DATE_ERROR = "Дата не может быть меньше даты последнего действия по договору";
        public ContractAction Exec(int contractId, decimal amount, int payTypeId, int branchId, int authorId, DateTime date, int? employeeId = null,
            ProcessingInfo processingInfo = null, OrderStatus? orderStatus = null)
        {
            if (amount <= 0)
                return null;

            //amount = Math.Round(amount, 2); -- нужно ли округлять ???
            if (processingInfo != null)
            {
                if (processingInfo.Amount <= 0)
                    throw new ArgumentException($"Свойство {nameof(processingInfo.Amount)} должно быть больше нуля", nameof(processingInfo));

                if (processingInfo.Reference <= 0)
                    throw new ArgumentException($"Свойство {nameof(processingInfo.Reference)} должно быть больше нуля", nameof(processingInfo));
            }

            if (employeeId.HasValue)
            {
                User employee = _userRepository.Get(employeeId.Value);
                if (employee == null)
                    throw new PawnshopApplicationException($"Пользователь(сотрудник(подотчетное лицо)) {employeeId.Value} не найден");
            }

            Contract contract = _contractRepository.Get(contractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractId} не найден");

            if (!contract.SettingId.HasValue && (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.SoldOut))
                throw new PawnshopApplicationException($"Договор {contract.ContractNumber} должен быть подписан");

            bool isInitialFee = contract.Status == ContractStatus.AwaitForInitialFee;

            if (contract.SettingId.HasValue
                && contract.Setting.InitialFeeRequired.HasValue
                && contract.Setting.InitialFeeRequired <= 0
                && !isInitialFee)
                throw new InvalidOperationException();

            if (!isInitialFee && contract.Actions.Count > 0 && date < contract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(BAD_DATE_ERROR);

            PayType payType = _payTypeRepository.Get(payTypeId);
            if (payType == null)
                throw new PawnshopApplicationException($"Тип оплаты {payTypeId} не найден");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь(автор) {authorId} не найден");

            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            ContractDuty contractDuty = _contractDutyService.GetContractDuty(
                    new ContractDutyCheckModel
                    {
                        ActionType = ContractActionType.Prepayment,
                        ContractId = contract.Id,
                        PayTypeId = payTypeId,
                        Date = date
                    });

            if (contractDuty == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDuty)} не будет null");

            if (contractDuty.Rows == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDuty)}.{nameof(contractDuty.Rows)} не будет null");

            if (contractDuty.Rows.Count == 0)
                throw new PawnshopApplicationException("Не получено action rows с getcontract duty чем ожидалось");

            List<ContractActionRow> contractActionRows = contractDuty.Rows;
            decimal calculatedAmount = amount;
            ContractAction additionalPrepaymentAction = null;
            if (contract.Status == ContractStatus.Signed || contract.Status == ContractStatus.SoldOut)
            {
                if (contractActionRows.Count > 1)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractActionRows)} будет только один элемент");

                ContractActionRow contractActionRow = contractActionRows.Single();
                contractActionRow.Cost = amount;
            }
            else
            {
                if (contractActionRows.Count > 2)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractActionRows)} будет только два элемента");

                Account depoAccount = _accountService.GetByAccountSettingCode(contract.Id, Constants.ACCOUNT_SETTING_DEPO);
                if (depoAccount == null)
                    throw new PawnshopApplicationException($"По договору {contract.Id} не найден счет по настройке {Constants.ACCOUNT_SETTING_DEPO}");

                Account depoMerchantAccount = _accountService.GetByAccountSettingCode(contract.Id, Constants.ACCOUNT_SETTING_DEPO_MERCHANT);
                if (depoMerchantAccount == null)
                    throw new PawnshopApplicationException($"По договору {contract.Id} не найден счет по настройке {Constants.ACCOUNT_SETTING_DEPO_MERCHANT}");

                ContractActionRow depoActionRow = contractActionRows.Where(r => r.CreditAccountId == depoAccount.Id).FirstOrDefault();
                if (depoActionRow == null)
                    throw new PawnshopApplicationException($"{nameof(contractActionRows)} должен был содержать row с кредитным счетом {depoAccount.Id}");

                ContractActionRow depoMerchantActionRow = contractActionRows.Where(r => r.CreditAccountId == depoMerchantAccount.Id).FirstOrDefault();
                if (depoMerchantActionRow == null)
                    throw new PawnshopApplicationException($"{nameof(contractActionRows)} должен был содержать row с кредитным счетом {depoMerchantAccount.Id}");

                decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id, date);
                if (!contract.RequiredInitialFee.HasValue)
                    throw new PawnshopApplicationException($"Договор {contract.Id} должен содержать {nameof(contract.RequiredInitialFee)}");

                decimal requiredInitialFee = contract.RequiredInitialFee.Value;
                if (depoMerchantBalance > requiredInitialFee)
                    throw new PawnshopApplicationException($"Договор {contract.Id} уже содержит необходимый первоначальный взнос, но его статус остался 'Ожидает первоначального взноса'");

                decimal depoMerchantBalanceAndAmount = depoMerchantBalance + amount;
                decimal calculatedDepoMerchantAmount = 0;
                if (depoMerchantBalanceAndAmount > requiredInitialFee)
                {
                    decimal initialFeeDifference = depoMerchantBalanceAndAmount - requiredInitialFee;
                    calculatedDepoMerchantAmount = amount - initialFeeDifference;
                }
                else
                {
                    calculatedDepoMerchantAmount = amount;
                }

                calculatedAmount = calculatedDepoMerchantAmount;
                depoMerchantActionRow.Cost = calculatedAmount;
                depoActionRow.Cost = calculatedAmount;
                if (calculatedDepoMerchantAmount < amount)
                {
                    decimal additionaPrepaymentSum = amount - calculatedDepoMerchantAmount;
                    ContractActionRow depoActionRowClone = new ContractActionRow
                    {
                        CreditAccountId = depoActionRow.CreditAccountId,
                        DebitAccountId = depoActionRow.DebitAccountId,
                        BusinessOperationSettingId = depoActionRow.BusinessOperationSettingId,
                        Cost = additionaPrepaymentSum,
                        LoanSubjectId = depoActionRow.LoanSubjectId,
                        OriginalPercent = depoActionRow.OriginalPercent,
                        PaymentType = depoActionRow.PaymentType,
                        Period = depoActionRow.Period,
                        Percent = depoActionRow.Percent,
                    };

                    var additionalPrepaymentActionRows = new ContractActionRow[] { depoActionRowClone };
                    additionalPrepaymentAction = new ContractAction
                    {
                        ActionType = ContractActionType.Prepayment,
                        AuthorId = authorId,
                        ContractId = contract.Id,
                        Cost = additionaPrepaymentSum,
                        TotalCost = additionaPrepaymentSum,
                        Rows = additionalPrepaymentActionRows,
                        Discount = contractDuty.Discount,
                        Reason = $"Поступление аванса по договору {contract.ContractNumber} от {date:dd.MM.yyyy}",
                        Date = contractDuty.Date,
                        PayTypeId = payTypeId,
                        IsInitialFee = false,
                        Data = new ContractActionData(),
                        EmployeeId = employeeId
                    };

                    if (processingInfo != null)
                    {
                        additionalPrepaymentAction.ProcessingType = processingInfo.Type;
                        additionalPrepaymentAction.ProcessingId = processingInfo.Reference;
                        additionalPrepaymentAction.Data.ProcessingBankName = processingInfo.BankName;
                        additionalPrepaymentAction.Data.ProcessingBankNetwork = processingInfo.BankNetwork;
                    }
                }
            }

            var prepaymentAction = new ContractAction
            {
                ActionType = ContractActionType.Prepayment,
                AuthorId = authorId,
                ContractId = contract.Id,
                Cost = calculatedAmount,
                TotalCost = calculatedAmount,
                Rows = contractActionRows.ToArray(),
                Discount = contractDuty.Discount,
                Reason = contractDuty.Reason,
                Date = contractDuty.Date,
                PayTypeId = payTypeId,
                IsInitialFee = contract.Status == ContractStatus.AwaitForInitialFee,
                Data = new ContractActionData(),
                EmployeeId = employeeId
            };

            if (additionalPrepaymentAction == null && processingInfo != null)
            {
                prepaymentAction.ProcessingType = processingInfo.Type;
                prepaymentAction.ProcessingId = processingInfo.Reference;
                prepaymentAction.Data.ProcessingBankName = processingInfo.BankName;
                prepaymentAction.Data.ProcessingBankNetwork = processingInfo.BankNetwork;
            }

            using (IDbTransaction transaction = _groupRepository.BeginTransaction())
            {
                _contractActionOperationService.Register(contract, prepaymentAction, authorId, branchId: branchId, orderStatus: orderStatus);
                if (additionalPrepaymentAction != null)
                {
                    additionalPrepaymentAction.ParentActionId = prepaymentAction.Id;
                    _contractActionOperationService.Register(contract, additionalPrepaymentAction, authorId, branchId: branchId);
                    additionalPrepaymentAction.ParentAction = prepaymentAction;
                    prepaymentAction.ChildActionId = additionalPrepaymentAction.Id;
                    _contractActionService.Save(prepaymentAction);
                }

                if (prepaymentAction.IsInitialFee == true)
                {
                    if (!contract.RequiredInitialFee.HasValue || contract.Status != ContractStatus.AwaitForInitialFee)
                        throw new PawnshopApplicationException("По договору не рассчитана сумма обязательного первоначального взноса, сделайте сначала операцию, которая рассчитывает и разрешает внесение первоначального взноса");

                    decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id, date);
                    if (contract.RequiredInitialFee <= depoMerchantBalance)
                    {
                        // перезагружаем договор
                        contract = _contractRepository.Get(contractId);
                        if (contract == null)
                            throw new PawnshopApplicationException($"Договор {contractId} не найден");

                        contract.Status = ContractStatus.PositionRegistration;
                        contract.PayedInitialFee = depoMerchantBalance;
                        _contractRepository.Update(contract);
                    }
                }

                transaction.Commit();
            }


            return additionalPrepaymentAction ?? prepaymentAction;
        }

        public ContractAction MovePrepayment(MovePrepayment prepaymentModel, int authorId, Group branch, ContractAction parentAction = null, bool autoApprove = true)
        {
            OrderStatus orderStatus = OrderStatus.WaitingForApprove;
            if (autoApprove)
                orderStatus = OrderStatus.Approved;

            if (prepaymentModel is null)
                throw new PawnshopApplicationException($"Ожидалось, что {nameof(prepaymentModel)} не будет null");

            if (prepaymentModel.SourceContractId == 0)
                throw new PawnshopApplicationException("Договор-отправитель не заполнен");

            if (prepaymentModel.RecipientContractId == 0)
                throw new PawnshopApplicationException("Договор-получатель не заполнен");

            if (prepaymentModel.SourceContractId == prepaymentModel.RecipientContractId)
                throw new PawnshopApplicationException("Договор-получатель не может быть договором-отправителем");

            var sourceContract = _contractService.Get(prepaymentModel.SourceContractId);
            var recipientContract = _contractService.Get(prepaymentModel.RecipientContractId);

            if (sourceContract is null)
                throw new PawnshopApplicationException("Договор-отправитель не найден");

            if (recipientContract is null)
                throw new PawnshopApplicationException("Договор-получатель не найден");

            if (sourceContract.Status != ContractStatus.Signed && sourceContract.Status != ContractStatus.BoughtOut)
                throw new PawnshopApplicationException("Договор-отправитель должен быть в статусе Подписан или Выкуплен");

            if (recipientContract.Status != ContractStatus.Signed)
                throw new PawnshopApplicationException("Договор-получатель должен быть в статусе Подписан");

            if (prepaymentModel.Amount <= 0)
                throw new PawnshopApplicationException("Сумма переноса должна быть больше 0");

            var prepaymentBalance = _contractService.GetPrepaymentBalance(prepaymentModel.SourceContractId, prepaymentModel.Date);

            if (prepaymentBalance == 0 && autoApprove)
                throw new PawnshopApplicationException("Сумма аванса равна 0, перенос невозможен");

            if (prepaymentModel.Amount > prepaymentBalance && autoApprove)
                throw new PawnshopApplicationException("Сумма переноса больше чем сумма на авансе, перенос невозможен");

            var sourceMovePrepaymentAction = new ContractAction
            {
                ActionType = ContractActionType.PrepaymentToTransit,
                AuthorId = authorId,
                ContractId = sourceContract.Id,
                TotalCost = prepaymentModel.Amount,
                Cost = prepaymentModel.Amount,
                Reason = $"Перенос суммы аванса с договора {sourceContract.ContractNumber} на договор {recipientContract.ContractNumber}",
                Date = prepaymentModel.Date,
                CreateDate = DateTime.Now,
                Files = prepaymentModel.Files,
                Note = prepaymentModel.Note
            };

            var recipientMovePrepaymentAction = new ContractAction
            {
                ActionType = ContractActionType.PrepaymentFromTransit,
                AuthorId = authorId,
                ContractId = recipientContract.Id,
                TotalCost = prepaymentModel.Amount,
                Cost = prepaymentModel.Amount,
                Reason = $"Перенос суммы аванса с договора {sourceContract.ContractNumber} на договор {recipientContract.ContractNumber}",
                Date = prepaymentModel.Date,
                CreateDate = DateTime.Now,
                Note = prepaymentModel.Note
            };

            try
            {
                using (var transaction = _contractActionService.BeginContractActionTransaction())
                {
                    var amountDict = new Dictionary<AmountType, decimal>
                    {
                        {
                            AmountType.ManualOrder, prepaymentModel.Amount
                        }
                    };

                    _contractActionService.Save(sourceMovePrepaymentAction);
                    _businessOperationService.Register(sourceContract, prepaymentModel.Date,
                        Constants.BO_PREPAYMENT_MOVE_TO_TRANSIT, branch, authorId, amountDict,
                        action: sourceMovePrepaymentAction , orderStatus: orderStatus);
                    _contractActionOperationService.Register(sourceContract, sourceMovePrepaymentAction, authorId,
                        branchId: branch.Id, false, orderStatus: orderStatus);

                    var recepientBranch = recipientContract.Branch;

                    _contractActionService.Save(recipientMovePrepaymentAction);
                    _businessOperationService.Register(recipientContract, prepaymentModel.Date,
                        Constants.BO_PREPAYMENT_MOVE_FROM_TRANSIT, recepientBranch, authorId, amountDict,
                        action: recipientMovePrepaymentAction, orderStatus: orderStatus);
                    _contractActionOperationService.Register(recipientContract, recipientMovePrepaymentAction, authorId,
                        branchId: recepientBranch.Id, false, orderStatus: orderStatus);

                    if (parentAction != null)
                        sourceMovePrepaymentAction.ParentActionId = parentAction.Id;

                    sourceMovePrepaymentAction.ChildActionId = recipientMovePrepaymentAction.Id;
                    recipientMovePrepaymentAction.ParentActionId = sourceMovePrepaymentAction.Id;

                    _contractActionService.Save(sourceMovePrepaymentAction);
                    _contractActionService.Save(recipientMovePrepaymentAction);

                    transaction.Commit();
                }

                _eventLog.Log(EventCode.MovePrepaymentSourceContract, EventStatus.Success, EntityType.Contract,
                    prepaymentModel.SourceContractId, userId: authorId);
                _eventLog.Log(EventCode.MovePrepaymentRecipientContract, EventStatus.Success, EntityType.Contract,
                    prepaymentModel.RecipientContractId, userId: authorId);
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.MovePrepaymentSourceContract, EventStatus.Failed, EntityType.Contract,
                    prepaymentModel.SourceContractId, responseData: e.Message, userId: authorId);

                throw new PawnshopApplicationException($"Перенос суммы аванса с договора {sourceContract.ContractNumber} на договор {recipientContract.ContractNumber} невозможен." +
                                                       $"Exception Message: {e.Message}");
            }

            if (orderStatus == OrderStatus.WaitingForApprove)
                return recipientMovePrepaymentAction;

            return sourceMovePrepaymentAction;
        }
    }
}
