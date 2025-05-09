using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Services.Contracts;
using Pawnshop.Web.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services
{
    public class InnerNotificationService : IInnerNotificationService
    {
        private readonly ContractRepository _contractRepository;
        private readonly IContractService _contractService;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        public InnerNotificationService(ContractRepository contractRepository, IContractService contractService,
            InnerNotificationRepository innerNotificationRepository)
        {
            _contractRepository = contractRepository;
            _contractService = contractService;
            _innerNotificationRepository = innerNotificationRepository;
        }
        public InnerNotification CreateNotification(int contractId, NotificationPaymentType type, decimal? notEnoughSum = null)
        {
            Contract contract = _contractRepository.GetOnlyContract(contractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractId} не найден");

            decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id);
            var message = type switch
            {
                NotificationPaymentType.Prolong => GetProlongText(contract),
                NotificationPaymentType.PartialProlong => GetPartialProlongText(contract, notEnoughSum),
                NotificationPaymentType.NotEnoughForExpense => GetNotEnoughForExpenseText(contract, notEnoughSum),
                NotificationPaymentType.MonthlyPayment => GetMonthlyPaymentText(contract),
                NotificationPaymentType.PartialMonthlyPayment => GetPartialMonthlyPaymentText(contract, notEnoughSum),
                NotificationPaymentType.Buyout => GetBuyoutText(contract),
                NotificationPaymentType.BuyoutWithExcessPrepayment => GetBuyoutWithExcessPrepayment(contract, depoBalance),
                NotificationPaymentType.Inscription => GetInscriptionText(contract),
                NotificationPaymentType.InitialFee => GetInitialFeeText(contract),
                NotificationPaymentType.PaymentAccepted => GetPaymentAcceptedText(contract, depoBalance),
                _ =>
                $"[ОШИБКА]По договору {contract.ContractNumber} имеется аванс {depoBalance} KZT. При освоении денег - ПРОИЗОШЛА ОШИБКА. Свяжитесь с Администратором портала!"
            };
            var innerNotification = new InnerNotification()
            {
                Message = message,
                CreateDate = DateTime.Now,
                CreatedBy = Constants.ADMINISTRATOR_IDENTITY,
                ReceiveBranchId = contract.BranchId,
                EntityType = EntityType.Contract,
                EntityId = contract.Id,
                Status = InnerNotificationStatus.Sent
            };

            return innerNotification;
        }

        private string GetProlongText(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            return $"Продление договора {contract.ContractNumber} от {DateTime.Now.Date:dd.MM.yyyy} по {DateTime.Now.Date.AddDays(30):dd.MM.yyyy} прошло успешно";
        }

        private string GetPartialProlongText(Contract contract, decimal? notEnoughSum)
        {
            if (!notEnoughSum.HasValue)
                throw new ArgumentNullException(nameof(notEnoughSum));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            decimal notEnough = Math.Round(notEnoughSum.Value, 2);
            return $"Продление договора {contract.ContractNumber}. Денег на авансе не хватило на полное погашение задолженности, произошла частичная оплата, для продления необходимо доплатить {notEnough} KZT";
        }

        private string GetNotEnoughForExpenseText(Contract contract, decimal? notEnoughSum)
        {
            if (!notEnoughSum.HasValue)
                throw new ArgumentNullException(nameof(notEnoughSum));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            decimal notEnough = Math.Round(notEnoughSum.Value, 2);
            return $"Погашение договора {contract.ContractNumber}. Денег на авансе не хватает на погашение дополнительных расходов по договору, частичная оплата не может быть осуществлена, для погашения задолженности необходимо доплатить {notEnough} KZT";
        }

        private string GetMonthlyPaymentText(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            return $"Ежемесячное погашение договора {contract.ContractNumber} от {contract.ContractDate.Date:dd.MM.yyyy} прошло успешно";
        }

        private string GetPartialMonthlyPaymentText(Contract contract, decimal? notEnoughSum = null)
        {
            if (!notEnoughSum.HasValue)
                throw new ArgumentNullException(nameof(notEnoughSum));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            decimal notEnough = Math.Round(notEnoughSum.Value, 2);
            return $"Ежемесячное погашение договора {contract.ContractNumber}. Денег на авансе не хватило на полное погашение задолженности, произошла частичная оплата, для ежемесячного погашения необходимо доплатить {notEnough} KZT";
        }

        private string GetBuyoutText(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            return $"Выкуп договора {contract.ContractNumber} от {contract.ContractDate:dd.MM.yyyy}";
        }

        private string GetBuyoutWithExcessPrepayment(Contract contract, decimal depoBalance)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));


            return $"Выкуп договора {contract.ContractNumber} от {contract.ContractDate:dd.MM.yyyy}, остался остаток на авансе {depoBalance} KZT";
        }

        private string GetInscriptionText(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            return $"По договору {contract.ContractNumber} получен аванс, но имеется исполнительная надпись, освоение денег НЕ ПРОИЗОШЛО. Свяжитесь с отделом взыскания!";
        }

        private string GetInitialFeeText(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id);
            return $"По договору {contract.ContractNumber} поступил первоначальный взнос, Текущий баланс счета с учетом суммы поступления составляет {depoMerchantBalance} KZT";
        }

        private string GetPaymentAcceptedText(Contract contract, decimal depoBalance)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            return $"По договору {contract.ContractNumber} поступил аванс, Текущий баланс счета с учетом суммы поступления составляет {depoBalance} KZT";
        }
    }
}
