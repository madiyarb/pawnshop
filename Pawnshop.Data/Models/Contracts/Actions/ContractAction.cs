using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Files;
using System.Collections.Generic;
using Pawnshop.Core.Exceptions;
using System.Linq;
using System.Reflection;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.PayOperations;
using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public class ContractAction : IEntity, ILoggableToEntity
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int AuthorId { get; set; }

        public ContractActionType ActionType { get; set; }
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public decimal TotalCost { get; set; }
        public bool? CategoryChanged { get; set; }
        public string Note { get; set; }

        [Required(ErrorMessage = "Поле \"Основание\" обязательно для заполнения")]
        public string Reason { get; set; }

        public ContractActionRow[] Rows { get; set; }

        public ContractActionData Data { get; set; }

        public ContractRefinanceConfig RefinanceConfig { get; set; }

        public List<FileRow> Files { get; set; } = new List<FileRow>();

        public List<ContractActionCheckValue> Checks { get; set; }

        public List<ContractCheckValue> ContractChecks = new List<ContractCheckValue>();

        /// <summary>
        /// Сотрудник(подотчетный)
        /// </summary>
        public int? EmployeeId { get; set; }

        /// <summary>
        /// Расход
        /// </summary>
        public ContractExpense Expense { get; set; }

        /// <summary>
        /// Информация о скидке(ах)
        /// </summary>
        public ContractDutyDiscount Discount { get; set; } = new ContractDutyDiscount();

        /// <summary>
        /// Идентификато расхода
        /// </summary>
        public int? ExpenseId { get; set; }

        /// <summary>
        /// Следующий договор (порожденный)
        /// </summary>
        public int? FollowedId { get; set; }

        /// <summary>
        /// Идентификатор онлайн оплаты
        /// </summary>
        public Int64? ProcessingId { get; set; }

        /// <summary>
        /// Тип платежной системы
        /// </summary>
        public ProcessingType? ProcessingType { get; set; }

        /// <summary>
        /// Идентификатор онлайн оплаты при освоении денег
        /// </summary>
        public int? OnlinePaymentId { get; set; }

        /// <summary>
        /// Родительское действие
        /// </summary>
        public int? ParentActionId { get; set; }

        public ContractAction ParentAction { get; set; }

        /// <summary>
        /// Порожденное действие
        /// </summary>
        public int? ChildActionId { get; set; }

        public ContractAction ChildAction { get; set; }

        /// <summary>
        /// Вид оплаты
        /// </summary>
        //[RequiredId(ErrorMessage = "Не выбран вид оплаты")]
        [CustomValidation(typeof(ContractAction), "PayTypeIdValidate")]
        public int? PayTypeId { get; set; }

        /// <summary>
        /// Операция оплаты 
        /// </summary>
        public int? PayOperationId { get; set; }

        /// <summary>
        /// Реквизит
        /// </summary>
        public int? RequisiteId { get; set; }

        /// <summary>
        /// Сумма для перевода по счету 
        /// </summary>
        public int? RequisiteCost { get; set; }

        /// <summary>
        /// Позиции 
        /// </summary>
        public List<ContractPosition> Positions { get; set; } = new List<ContractPosition>();

        /// <summary>
        /// Автомобили
        /// </summary>
        public List<Car> Cars { get; set; }

        /// <summary>
        /// Недвижимость
        /// </summary>
        public List<Realty> Realties { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Сумма дополнительных расходов
        /// </summary>
        public decimal? ExtraExpensesCost { get; set; }

        /// <summary>
        /// Сумма дополнительных расходов
        /// </summary>
        public List<int> ExtraExpensesIds { get; set; }

        /// <summary>
        /// Первоначальный взнос
        /// </summary>
        public bool? IsInitialFee { get; set; }

        /// <summary>
        /// Субъект
        /// </summary>
        public int? LoanSubjectId { get; set; }

        public bool IsFromOnlinePaymentJob { get; set; } = false;

        /// <summary>
        /// Статус действия
        /// </summary>
        public ContractActionStatus? Status { get; set; }

        /// <summary>
        /// Ссылка на реализацию
        /// </summary>
        public int? SellingId { get; set; }

        /// <summary>
        /// Действие происходит из реализации
        /// </summary>
        public bool isFromSelling { get; set; } = false;
        /// <summary>
        /// Идентификатор причины выкупа
        /// </summary>
        public int? BuyoutReasonId { get; set; } = null;

        public bool HasPercentAdjustment => Discount?.Discounts
            .Where(x => x.ContractDiscountId.HasValue).Any(x => x.ContractDiscount.IsTypical && x.ContractDiscount.PersonalDiscount.ActionType == ActionType && (x.TotalLoanPercentAdjustment != 0 || x.TotalPenaltyPercentAdjustment != 0)) ?? false;

        [JsonIgnore]
        public ContractActionType? PreviousContractActionType { get; set; }

        public bool? IsReceivable { get; set; }

        //public List<int> OrderIdsForFiscalCheck { get; set; }

        public int GetLinkedEntityId()
        {
         
            return ContractId;
        }

        /// <summary>
        /// Список погашанных кредитов
        /// </summary>
        public List<ContractKdnDetail> ClientOtherPaymentsInfo { get; set; }

        /// <summary>
        /// Id продукта для Дочернего договора
        /// </summary>
        public int? ChildSettingId { get; set; }

        /// <summary>
        /// Новый период для Дочернего договора
        /// </summary>
        public int? ChildLoanPeriod { get; set; }

        /// <summary>
        /// Id созаемщика для Дочернего договора
        /// </summary>
        public int? ChildSubjectId { get; set; }

        public bool BuyoutCreditLine { get; set; }

        public int? ClientDefermentId { get; set; }

        /// <summary>
        /// Освоение денег с авансового баланса
        /// </summary>
        public void TakeMoneyFromPrepayment(Contract contract, Configuration configuration = null, List<AmountType> restrictedToUse = null)
        {
            int DebitId = 0;
            if (contract.Branch == null) throw new PawnshopApplicationException("Не найдены настройки отделения для договора");
            if (configuration != null ? !configuration.CashOrderSettings.ProfitlessAccountId.HasValue : !contract.Branch.Configuration.CashOrderSettings.ProfitlessAccountId.HasValue) throw new PawnshopApplicationException("Счёт убыли не настроен. Настройте или обратитесь к администратору.");

            int ProfitlessAccountId = configuration == null ? contract.Branch.Configuration.CashOrderSettings.ProfitlessAccountId.Value : configuration.CashOrderSettings.ProfitlessAccountId.Value;

            if (configuration == null)
            {
                if (contract.Branch.Configuration.CashOrderSettings.Get(contract.CollateralType).PrepaymentSettings == null) throw new PawnshopApplicationException($"Не найдены авансовые настройки {Enum.GetName(typeof(CollateralType), contract.CollateralType)} для филиала");
            }
            else
            {
                if (configuration.CashOrderSettings.Get(contract.CollateralType).PrepaymentSettings == null) throw new PawnshopApplicationException($"Не найдены авансовые настройки {Enum.GetName(typeof(CollateralType),contract.CollateralType)} для филиала или организации");
            }

            DebitId = (int) (configuration?.CashOrderSettings?.Get(contract.CollateralType)?.PrepaymentSettings?.CreditId ?? contract.Branch.Configuration.CashOrderSettings.Get(contract.CollateralType).PrepaymentSettings.CreditId);

            if (DebitId == 0) throw new PawnshopApplicationException("Счет Дебета для аванса не определён!");

            var PrepaymentCost = contract.ContractData.PrepaymentCost;

            if (Data == null)
            {
                Data = new ContractActionData();
            }
            Data.PrepaymentUsed = 0;

            List<ContractActionRow> newrows = new List<ContractActionRow>();
            foreach (var row in Rows.ToList().OrderByDescending(x => x.PaymentType))
            {
                if (restrictedToUse != null)
                {
                    if (restrictedToUse.Contains(row.PaymentType)) 
                    {
                        newrows.Add(row);
                        continue;
                    }
                }

                ContractActionRow newrow = new ContractActionRow
                {
                    ActionId = row.ActionId,
                    PaymentType = row.PaymentType,
                    CreditAccountId = row.CreditAccountId,
                    DebitAccountId = DebitId
                };

                newrow.Cost = PrepaymentCost > row.Cost ? row.Cost : PrepaymentCost;
                row.Cost -= newrow.Cost;
                row.DebitAccountId = (contract.InscriptionId > 0 && contract.Inscription.Status == InscriptionStatus.Executed) ? ProfitlessAccountId : row.DebitAccountId;
                PrepaymentCost -= newrow.Cost;
                Data.PrepaymentUsed += newrow.Cost;
                if (newrow.Cost > 0)
                {
                    if (row.Cost == 0)
                    {
                        newrow.Period = row.Period;
                        newrow.Percent = row.Percent;
                        newrows.Add(newrow);
                    }
                    else
                    {
                        newrows.Add(newrow);
                        newrows.Add(row);
                    }
                }
                else
                {
                    newrows.Add(row);
                }
            }

            contract.ContractData.PrepaymentCost -= Data.PrepaymentUsed;
            Rows = newrows.OrderBy(x=>x.PaymentType).ToArray();
        }

        public static ValidationResult PayTypeIdValidate(int? value, ValidationContext context)
        {
            var contractAction = (ContractAction)context.ObjectInstance;
            var actionTypes = new List<ContractActionType>
            {
                ContractActionType.Prolong, ContractActionType.Buyout, ContractActionType.BuyoutRestructuringCred, ContractActionType.PartialBuyout,
                ContractActionType.Sign, ContractActionType.Selling, ContractActionType.ReTransfer, ContractActionType.MonthlyPayment, 
                ContractActionType.Addition, ContractActionType.Prepayment, ContractActionType.PrepaymentReturn
            };

            if (actionTypes.Contains(contractAction.ActionType) && !contractAction.PayTypeId.HasValue)
            {
                return new ValidationResult("Не выбран вид оплаты");
            }
            return ValidationResult.Success;
        }
    }    
}