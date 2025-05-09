using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Contracts.Postponements;
using Pawnshop.Data.Models.Contracts.Discounts;
using System.Linq;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Transfers;
using IEntity = Pawnshop.Core.IEntity;
using Pawnshop.Data.Models.ClientDeferments;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Договор займа
    /// </summary>
    public class Contract : IEntity, IOwnable, IContract
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        [RequiredId(ErrorMessage = "Поле клиент обязательно для заполнения")]
        public int ClientId { get; set; }

        public Client Client { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата договора обязательно для заполнения")]
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Дата подписания
        /// </summary>
        public DateTime? SignDate { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Вид удержания процентов
        /// </summary>
        public PercentPaymentType PercentPaymentType { get; set; }

        /// <summary>
        /// Дата возврата
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата возврата обязательно для заполнения")]
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Первоначальная дата возврата
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле первоначальная дата возврата обязательно для заполнения")]
        public DateTime OriginalMaturityDate { get; set; }

        /// <summary>
        /// Оценочная стоимость
        /// </summary>
        public int EstimatedCost { get; set; }

        /// <summary>
        /// Ссуда
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Срок залога (дней)
        /// </summary>
        public int LoanPeriod { get; set; }

        /// <summary>
        /// Процент кредита
        /// </summary>
        [Range(0, 100, ErrorMessage = "Поле процент кредита должно иметь значение от 0 до 100")]
        public decimal LoanPercent { get; set; }

        /// <summary>
        /// Стоимость процента кредита
        /// </summary>
        public decimal LoanPercentCost { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Данные договора
        /// </summary>
        [Required(ErrorMessage = "Поле данные договора обязательно для заполнения")]
        public ContractData ContractData { get; set; }

        /// <summary>
        /// Специфичные поля договора
        /// </summary>
        public GoldContractSpecific ContractSpecific { get; set; }

        /// <summary>
        /// Идентификатор владельца
        /// </summary>
        [RequiredId(ErrorMessage = "Поле владелец обязательно для заполнения")]
        public int OwnerId { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Позиции договора
        /// </summary>
        public List<ContractPosition> Positions { get; set; } = new List<ContractPosition>();

        /// <summary>
        /// Файлы договора
        /// </summary>
        public List<FileRow> Files { get; set; } = new List<FileRow>();

        /// <summary>
        /// Статус договора
        /// </summary>
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        /// <summary>
        /// Бизнес статус договора
        /// </summary>
        public ContractDisplayStatus DisplayStatus { get; set; }

        /// <summary>
        /// Дата продления
        /// </summary>
        public DateTime? ProlongDate { get; set; }

        /// <summary>
        /// Дата передачи
        /// </summary>
        public DateTime? TransferDate { get; set; }

        /// <summary>
        /// Дата первой оплаты
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }

        /// <summary>
		/// Филиал, в котором создан договор
		/// </summary>
		[RequiredId(ErrorMessage = "Поле филиал обязательно для заполнения")]
        public int BranchId { get; set; }

        /// <summary>
        /// Филиал, в котором создан договор
        /// </summary>
        public Group Branch { get; set; }

        /// <summary>
        /// Автор договора
        /// </summary>
        [RequiredId(ErrorMessage = "Поле автор обязательно для заполнения")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор договора
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Договор должен быть подписан
        /// </summary>
        public bool Locked { get; set; } = false;

        /// <summary>
        /// True, если договор создан сегодня
        /// </summary>
        public bool CreatedToday => ContractDate.Date == DateTime.Now.Date;

        /// <summary>
        /// Журнал действий
        /// </summary>
        public List<ContractAction> Actions { get; set; } = new List<ContractAction>();

        /// <summary>
        /// Примечания
        /// </summary>
        public List<ContractNote> Notes { get; set; } = new List<ContractNote>();

        /// <summary>
        /// График погашения
        /// </summary>
        public List<ContractPaymentSchedule> PaymentSchedule { get; set; } = new List<ContractPaymentSchedule>();
        /// <summary>
        /// Реструктиуризированный график погашения
        /// </summary>
        public List<RestructuredContractPaymentSchedule> RestructedPaymentSchedule { get; set; } = new List<RestructuredContractPaymentSchedule>();

        /// <summary>
        /// Признак рестуктуризации
        /// </summary>
        public bool IsContractRestructured { get; set; } = false;

        /// <summary>
        /// Это период отсрочки
        /// </summary>
        public bool IsDefermentPeriod { get; set; }
        /// <summary>
        /// Расходы
        /// </summary>
        public List<ContractExpense> Expenses { get; set; } = new List<ContractExpense>();

        /// <summary>
        /// Идентификатор договора до частичного гашения
        /// </summary>
        public int? PartialPaymentParentId { get; set; }

        /// <summary>
        /// Номер пула в СФК
        /// </summary>
        public int? PoolNumber { get; set; }

        /// <summary>
        /// Дата выкупа
        /// </summary>
        public DateTime? BuyoutDate { get; set; }

        /// <summary>
        /// Причина выкупа
        /// </summary>
        public int? BuyoutReasonId { get; set; }

        /// <summary>
        /// Вид аннуитета
        /// </summary>
        public AnnuityType? AnnuityType { get; set; }

        /// <summary>
        /// Канал привлечения
        /// </summary>
        [RequiredId(ErrorMessage = "Поле канал привлечения обязательно для заполнения")]
        public int? AttractionChannelId { get; set; }

        /// <summary>
        /// Исполнительная надпись
        /// </summary>
        public int? InscriptionId { get; set; }
        public Inscription Inscription { get; set; }

        public List<ContractPostponement> Postponements { get; set; } = new List<ContractPostponement>();

        /// <summary>
        /// Идентификатор договора в CRM
        /// </summary>
        public int? CrmId { get; set; }

        /// <summary>
        /// Скидки
        /// </summary>
        public List<ContractDiscount> Discounts { get; set; } = new List<ContractDiscount>();

        /// <summary>
        /// ГЭСВ (годовая эффективная ставка вознаграждения)
        /// </summary>
        public decimal? APR { get; set; }

        /// <summary>
        /// Остаток основного долга
        /// </summary>
        public decimal LeftLoanCost { get; set; }


        public List<ContractCheckValue> Checks = new List<ContractCheckValue>();

        private int? parentId = null;
        public int? ParentId
        {
            get { return parentId; }
            set { parentId = value == 0 || value is null ? null : value; }
        }
        public Contract ParentContract { get; set; }

        /// <summary>
        /// Идентификатор закрытого договора
        /// </summary>
        public int? ClosedParentId { get; set; }

        //public int? DelayDayCountForToday => CalculateDelayDays();

        public void CheckEstimatedAndLoanCost()
        {
            var errors = new List<string>();

            if (EstimatedCost < LoanCost) errors.Add("Сумма оценки договора не может быть меньше ссуды");
            if (Positions != null && Positions.Count > 0)
            {
                Positions.ForEach(position =>
                {
                    if (position.EstimatedCost < position.LoanCost) errors.Add($"Сумма оценки по позиции{position.Position.Name} не может быть меньше ссуды");
                });
            }

            if (errors.Count > 0)
            {
                throw new PawnshopApplicationException(errors.ToArray());
            }
        }

        /// <summary>
        /// Идентификатор сделки-оплаты в CRM
        /// </summary>
        public int? CrmPaymentId { get; set; }

        /// <summary>
        /// Вид продукта
        /// </summary>
        public int? ProductTypeId { get; set; }

        /// <summary>
        /// Вид продукта
        /// </summary>
        public LoanProductType ProductType { get; set; }

        /// <summary>
        /// Настройка/продукт
        /// </summary>
        public int? SettingId { get; set; }

        /// <summary>
        /// Настройка/продукт
        /// </summary>
        public LoanPercentSetting Setting { get; set; }

        /// <summary>
        /// Субъекты договора
        /// </summary>
        public List<ContractLoanSubject> Subjects { get; set; }

        /// <summary>
        /// Минимальный (обязательный) первоначальный взнос(по продукту) 
        /// </summary>
        public decimal? MinimalInitialFee { get; set; }

        /// <summary>
        /// Требуемый первоначальный взнос(обговаривается с клиентом, не меньше MinimalInitialFee)
        /// </summary>
        public decimal? RequiredInitialFee { get; set; }

        /// <summary>
        /// Оплаченный первоначальный взнос
        /// </summary>
        public decimal? PayedInitialFee { get; set; }

        /// <summary>
        /// Идентификатор цели кредита
        /// </summary>
        public int? LoanPurposeId { get; set; }

        /// <summary>
        /// Прочая цель кредита
        /// </summary>
        public string OtherLoanPurpose { get; set; }

        /// <summary>
        /// Идентификатор сфер деятельности
        /// если Цель кредита - бизнеса/исвестиции/пополнение оборотных средств/инвестиции и пополнение ОС
        /// </summary>
        public int? BusinessLoanPurposeId { get; set; }

        /// <summary>
        /// Идентификатор для физических лиц (ОКЭД)
        /// если Цель кредита - бизнеса
        /// </summary>
        public int? OkedForIndividualsPurposeId { get; set; }

        /// <summary>
        /// Идентификатор целевой цели кредита
        /// если Цель кредита - бизнеса
        /// </summary>
        public int? TargetPurposeId { get; set; }

        public bool IsBuyCar => ProductTypeId.HasValue && ProductType != null && ProductType.Code == "BUYCAR";

        /// <summary>
        /// Переводы
        /// </summary>
        public List<ContractTransfer> ContractTransfers { get; set; }

        /// <summary>
        /// Тип договора
        /// </summary>
        public int ContractTypeId { get; set; }

        /// <summary>
        /// Тип срочности договора
        /// </summary>
        public int PeriodTypeId { get; set; }

        /// <summary>
        /// Дата следующей оплаты
        /// </summary>
        public DateTime? NextPaymentDate { get; set; }

        public bool IsOffBalance { get; set; }

        public List<IPaymentScheduleItem> GetSchedule()
        {
            return PaymentSchedule.OfType<IPaymentScheduleItem>().ToList();
        }

        public List<RestructuredContractPaymentSchedule> GetRestructuredSchedule()
        {
            return RestructedPaymentSchedule;
        }

        public ClientDeferment ClientDeferment { get; set; }

        /// <summary>
        /// ГРНЗ+Марка+Модель авто
        /// </summary>
        public string grnzCollat { get; set; }
        /// <summary>
        /// Наличие обременения
        /// </summary>
        public bool HasEncumbrance { get; set; }
        /// <summary>
        /// Схема очередности погашения
        /// </summary>
        public int PaymentOrderSchema { get; set; }
        /// <summary>
        /// Ссылка на подписанта с документом
        /// </summary>
        public int? SignerId { get; set; }
        public ClientSigner Signer { get; set; }
        /// <summary>
        /// Дата заседания кредитного комитета
        /// </summary>
        public DateTime? LCDate { get; set; }
        /// <summary>
        /// Номер решения кредитного комитета
        /// </summary>
        public string LCDecisionNumber { get; set; }

        public List<ContractRate> ContractRates { get; set; }
        public bool UsePenaltyLimit { get; set; }

        /// <summary>
        /// Является ли пенсионером владелец контракта - только для ContractController/card
        /// </summary>
        public bool? isPensioner { get; set; }

        public ContractClass ContractClass { get; set; }
        public int? CreditLineId { get; set; }
        public decimal MaxCreditLineCost { get; set; }

        //отсрочка по оплате ОД для смешанных графиков (в днях)
        public int? DebtGracePeriod { get; set; }
        public string CollectionStatusCode { get; set; }
        public int DelayDays { get; set; }
        public FirstTranche FirstTranche { get; set; }
        public bool IsFromMobileApp { get; set; }

        /// <summary>
        /// Создан в онлайне 
        /// </summary>
        public bool CreatedInOnline { get; set; }
        public bool CanEditFirstPaymentDate { get; set; }

        /// <summary>
        /// Идентификатор Аукциона
        /// </summary>
        public int? AuctionId { get; set; }
    }
}