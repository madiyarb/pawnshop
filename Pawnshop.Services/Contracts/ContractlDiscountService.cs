using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pawnshop.Services.Contracts
{
    public class ContractlDiscountService : IContractDiscountService
    {
        private readonly ContractRepository _contractRepository;
        private readonly UserRepository _userRepository;
        private readonly PersonalDiscountRepository _personalDiscountRepository;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly VehicleBlackListRepository _vehicleBlackListRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly DiscountRepository _discountRepository;
        private readonly IContractService _contractService;
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly ClientsBlackListRepository _clientsBlackListRepository;
        public ContractlDiscountService(ContractRepository contractRepository,
            UserRepository userRepository, PersonalDiscountRepository personalDiscountRepository,
            VehicleBlackListRepository vehicleBlackListRepository, IContractService contractService,
            ContractDiscountRepository contractDiscountRepository, ContractActionRepository contractActionRepository,
            DiscountRepository discountRepository, InscriptionRepository inscriptionRepository, ClientsBlackListRepository clientsBlackListRepository)
        {
            _contractRepository = contractRepository;
            _userRepository = userRepository;
            _personalDiscountRepository = personalDiscountRepository;
            _vehicleBlackListRepository = vehicleBlackListRepository;
            _contractDiscountRepository = contractDiscountRepository;
            _contractService = contractService;
            _contractActionRepository = contractActionRepository;
            _discountRepository = discountRepository;
            _inscriptionRepository = inscriptionRepository;
            _clientsBlackListRepository = clientsBlackListRepository;
        }

        public void Create(ContractDiscount contractDiscount, int authorId)
        {
            if (contractDiscount == null)
                throw new ArgumentNullException(nameof(contractDiscount));

            contractDiscount.EndDate = contractDiscount.EndDate.Date.AddDays(1).AddSeconds(-1);
            contractDiscount.BeginDate = contractDiscount.BeginDate.Date;
            ValidateContractDiscount(contractDiscount);
            Contract contract = _contractRepository.Get(contractDiscount.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractDiscount.Id} не найден");

            if (contract.Status != ContractStatus.Signed)
                throw new PawnshopApplicationException($"Договор {contract.Id} не является действующим");

            if (contractDiscount.Id == 0 && contractDiscount.ContractActionId.HasValue)
                throw new PawnshopApplicationException($"Нельзя привязать действие договора к новой скидке");

            if (contract.InscriptionId.HasValue)
            {
                Inscription inscription = _inscriptionRepository.Get(contract.InscriptionId.Value);
                if (inscription == null)
                    throw new PawnshopApplicationException($"Исполнительная надпись {contract.InscriptionId.Value} не найдена");

                if (inscription.Status != InscriptionStatus.Denied)
                    throw new PawnshopApplicationException(
                        $"По договору {contract.ContractNumber} невозможно создать скидку, так как имеется действующая исполнительная надпись");

            }

            ValidateContractDiscountSums(contractDiscount, contract);
            ContractDiscount contractDiscountFromDB = null;
            if (contractDiscount.Id != 0)
            {
                contractDiscountFromDB = _contractDiscountRepository.Get(contractDiscount.Id);
                if (contractDiscountFromDB == null)
                    throw new PawnshopApplicationException($"Скидка {nameof(contractDiscount.Id)} не найдена");

                if (contractDiscountFromDB.EndDate < DateTime.Now)
                    throw new PawnshopApplicationException("Нельзя изменять просроченную скидку!");

                if (contractDiscountFromDB.ContractActionId.HasValue)
                    throw new PawnshopApplicationException("Нельзя изменять скидку привязанную к определенному договору");

                if (contractDiscountFromDB.ContractId != contractDiscount.ContractId)
                    throw new PawnshopApplicationException("Договоры присланной скидки и скидки из базы отличаются");

                contractDiscount.AuthorId = contractDiscountFromDB.AuthorId;
                contractDiscount.CreateDate = contractDiscountFromDB.CreateDate;
            }

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            List<ContractDiscount> contractDiscounts = _contractDiscountRepository.GetByContractId(contract.Id);
            if (contractDiscounts == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDiscounts)} не будет null");

            if (contractDiscounts.Any(d => d.IsTypical && (d.PersonalDiscount == null || !d.PersonalDiscountId.HasValue)))
                throw new PawnshopApplicationException($"{nameof(contractDiscounts)} содержит пустую персональную скидку для типичных скидок");

            if (contractDiscounts.Any(d => !d.IsTypical && d.PersonalDiscountId.HasValue))
                throw new PawnshopApplicationException($"{nameof(contractDiscounts)} содержит непустую персональную скидку для нетипичных скидок");

            List<ContractDiscount> activeContractDicounts = contractDiscounts.Where(d => d.Status == ContractDiscountStatus.Accepted).ToList();
            PersonalDiscount personalDiscount = null;
            if (contractDiscount.IsTypical)
            {
                if (!contractDiscount.PersonalDiscountId.HasValue)
                    throw new ArgumentException($"Свойство {nameof(contractDiscount.PersonalDiscountId)} не должно быть пустым, так как это скидка типичная", nameof(contractDiscount));

                personalDiscount = _personalDiscountRepository.Get(contractDiscount.PersonalDiscountId.Value);
                if (personalDiscount == null)
                    throw new PawnshopApplicationException($"Персональная скидка {contractDiscount.PersonalDiscountId.Value} не найдена");

                if (contract.ContractClass != ContractClass.Tranche)
                {
                    ContractPosition carPosition = contract.Positions.FirstOrDefault();
                    if (carPosition == null)
                        throw new PawnshopApplicationException($"У договора нет позиции-машины");

                    Position position = carPosition.Position;
                    if (position == null)
                        throw new PawnshopApplicationException($"Ожидалось что {position} не будет null");

                    IBodyNumber bodyNumber = position as IBodyNumber;
                    if (bodyNumber == null)
                        throw new PawnshopApplicationException($"{nameof(bodyNumber)} не реализует интерфейс {nameof(IBodyNumber)}");

                    var blackListVehicle = _vehicleBlackListRepository.Find(bodyNumber);
                    if (blackListVehicle != null)
                    {
                        throw new PawnshopApplicationException("Нельзя изменять категорию аналитики на - с правом вождения, так как машина находится в списке черных VIN-кодов!");
                    }
                }

                List<ContractDiscount> activeContractDiscountsWithSameName =
                    activeContractDicounts.Where(d => d.IsTypical && d.PersonalDiscount.ActionType == personalDiscount.ActionType).ToList();

                if (activeContractDiscountsWithSameName.Count > 0)
                {
                    if (activeContractDiscountsWithSameName.Count > 1)
                        throw new PawnshopApplicationException($"Существуют {activeContractDiscountsWithSameName.Count} активных скидок для данного типа действия {personalDiscount.ActionType}, невозможный случай(должно быть не более одного), обратитесь в тех. поддержку");

                    ContractDiscount activeContractDiscount = activeContractDiscountsWithSameName.Single();
                    if (activeContractDiscount.Id != contractDiscount.Id)
                        throw new PawnshopApplicationException($"Активная скидка для данного типа действия {personalDiscount.ActionType} уже существует");
                }
            }
            else
            {
                if (contractDiscount.PersonalDiscountId.HasValue)
                    throw new PawnshopApplicationException("Скидка содержит информацию о персональных скидках, хотя она является нетиповой");

                List<ContractDiscount> activeAtypicalContractDiscounts = activeContractDicounts.Where(d => !d.IsTypical).ToList();
                if (activeAtypicalContractDiscounts.Count > 0)
                {
                    if (activeAtypicalContractDiscounts.Count > 1)
                        throw new PawnshopApplicationException($"Существуют {activeAtypicalContractDiscounts.Count} нетиповых активных скидок(должно быть не более одного), невозможный случай, обратитесь в тех. поддержку");

                    ContractDiscount activeContractDiscount = activeAtypicalContractDiscounts.Single();
                    if (activeContractDiscount.Id != contractDiscount.Id)
                        throw new PawnshopApplicationException("Активная нетиповая скидка уже существует по данному договору");
                }


                if (contractDiscount.DebtPenaltyDiscountSum == 0
                && contractDiscount.PercentDiscountSum == 0
                && contractDiscount.OverduePercentDiscountSum == 0
                && contractDiscount.PercentPenaltyDiscountSum == 0)
                    throw new ArgumentException(
                        $"Хотя бы одно из свойств {nameof(contractDiscount.PercentDiscountSum)}," +
                        $" {nameof(contractDiscount.DebtPenaltyDiscountSum)}," +
                        $" {nameof(contractDiscount.OverduePercentDiscountSum)}," +
                        $" {nameof(contractDiscount.PercentPenaltyDiscountSum)}" +
                        $" должны быть больше нуля", nameof(contractDiscount));
            }

            if (contractDiscount.Id == 0)
            {
                contractDiscount.AuthorId = author.Id;
                contractDiscount.CreateDate = DateTime.Now;
                _contractDiscountRepository.Insert(contractDiscount);
            }
            else
            {
                _contractDiscountRepository.Update(contractDiscount);
            }
        }

        public void Delete(int id)
        {
            ContractDiscount contractDiscount = Get(id);
            if (contractDiscount.Status != ContractDiscountStatus.Accepted)
                throw new PawnshopApplicationException("Нельзя удалить скидку из-за статуса");
            if (contractDiscount.ContractActionId.HasValue)
                throw new PawnshopApplicationException("Нельзя удалить данную скидку, так как она привязана в опредленному действию договора");

            _contractDiscountRepository.Delete(id);
        }

        public List<ContractDiscount> GetListByContractActionId(int contractActionId)
        {
            ContractAction contractAction = _contractActionRepository.Get(contractActionId);
            if (contractAction == null)
                throw new PawnshopApplicationException($"Действие договора {contractAction.Id}");

            List<ContractDiscount> contractDiscounts = _contractDiscountRepository.GetByContractActionId(contractAction.Id);
            if (contractDiscounts == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDiscounts)} не будет null");

            foreach (ContractDiscount contractDiscount in contractDiscounts)
            {
                if (contractDiscount == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDiscount)} не будет null");
            }

            return contractDiscounts;
        }

        public ContractDiscount Get(int id)
        {
            ContractDiscount contractDiscount = _contractDiscountRepository.Get(id);
            if (contractDiscount == null)
                throw new PawnshopApplicationException($"Скидка {id} не найдена");

            return contractDiscount;
        }

        public PersonalDiscount GetPersonalDiscountById(int personalDiscountId){
             var personalDiscount =_personalDiscountRepository.Get(personalDiscountId);

            if (personalDiscount == null)
                throw new PawnshopApplicationException($"Персональная скидка {personalDiscountId} не найдена");

            return personalDiscount;
        }

        private void ValidateContractDiscount(ContractDiscount contractDiscount)
        {
            if (contractDiscount == null)
                throw new ArgumentNullException(nameof(contractDiscount));

            if (contractDiscount.BeginDate.Date != contractDiscount.EndDate.Date)
                throw new ArgumentException($"Свойство {nameof(contractDiscount.BeginDate)} должно быть равно свойству {nameof(contractDiscount.EndDate)}", nameof(contractDiscount));
            if (contractDiscount.BeginDate != DateTime.Now.Date)
                throw new ArgumentException($"Свойство {nameof(contractDiscount.BeginDate)} должно быть равно дате текущего дня");
            if (contractDiscount.ContractActionId.HasValue)
                throw new ArgumentException("Нельзя сохранить скидку с определенным действием договора", nameof(contractDiscount.ContractActionId));
            if (contractDiscount.DeleteDate.HasValue)
                throw new ArgumentException("Чтобы удалить скидку, воспользуйтесь функцией удаления скидок", nameof(contractDiscount.DeleteDate));
        }

        private void ValidateContractDiscountSums(ContractDiscount contractDiscount, Contract contract)
        {
            if (contractDiscount == null)
                throw new ArgumentNullException(nameof(contractDiscount));
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var validationErrors = new List<string>();
            if (contractDiscount.DebtPenaltyDiscountSum < 0)
                validationErrors.Add($"Свойство {nameof(contractDiscount.DebtPenaltyDiscountSum)} должно быть положительным числом(больше или равно нулю)");
            if (contractDiscount.PercentDiscountSum < 0)
                validationErrors.Add($"Свойство {nameof(contractDiscount.PercentDiscountSum)} должно быть положительным числом(больше или равно нулю)");
            if (contractDiscount.PercentPenaltyDiscountSum < 0)
                validationErrors.Add($"Свойство {nameof(contractDiscount.PercentPenaltyDiscountSum)} должно быть положительным числом(больше или равно нулю)");
            if (contractDiscount.OverduePercentDiscountSum < 0)
                validationErrors.Add($"Свойство {nameof(contractDiscount.OverduePercentDiscountSum)} должно быть положительным числом(больше или равно нулю)");
            if (validationErrors.Count > 0)
                throw new PawnshopApplicationException(validationErrors.ToArray());

            decimal debtPenaltyDiscountSum = _contractService.GetPenyAccountBalance(contract.Id);
            decimal percentDiscountSum = _contractService.GetProfitBalance(contract.Id);
            decimal percentPenaltyDiscountSum = _contractService.GetPenyProfitBalance(contract.Id);
            decimal overduePercentDiscountSum = _contractService.GetOverdueProfitBalance(contract.Id);

            if (contractDiscount.DebtPenaltyDiscountSum > debtPenaltyDiscountSum)
                validationErrors.Add($"Сумма предоставленной скидки больше чем сумма остатка на счете пени по основному долгу");
            if (contractDiscount.PercentDiscountSum > percentDiscountSum)
                validationErrors.Add($"Сумма предоставленной скидки больше чем сумма остатка на счете начисленных процентов");
            if (contractDiscount.PercentPenaltyDiscountSum > percentPenaltyDiscountSum)
                validationErrors.Add($"Сумма предоставленной скидки больше чем сумма остатка на счете пени по процентам");
            if (contractDiscount.OverduePercentDiscountSum > overduePercentDiscountSum)
                validationErrors.Add($"Сумма предоставленной скидки больше чем сумма остатка на счете просроченных процентов");
            if (validationErrors.Count > 0)
                throw new PawnshopApplicationException(validationErrors.ToArray());
        }
    }
}
