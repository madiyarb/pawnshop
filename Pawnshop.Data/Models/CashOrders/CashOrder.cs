using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Contracts.Actions;
using IEntity = Pawnshop.Core.IEntity;
using Newtonsoft.Json;
using Pawnshop.Data.Models.TasOnline;

namespace Pawnshop.Data.Models.CashOrders
{
    /// <summary>
    /// Кассовый ордер
    /// </summary>
    public class CashOrder : IEntity, IOwnable, IOrder
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тип кассового ордера
        /// </summary>
        public OrderType OrderType { get; set; }

        /// <summary>
        /// Номер кассового ордера
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Дата кассового ордера
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата кассового ордера обязательно для заполнения")]
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Сумма
        /// </summary>
        [Range(1, 99999999999999, ErrorMessage = "Сумма должна быть больше нуля")]
        public decimal OrderCost { get; set; }

        /// <summary>
        /// Идентификатор счета дебета
        /// </summary>
        [CustomValidation(typeof(IOrder), "DebitAccountIdValidate")]
        public int? DebitAccountId { get; set; }

        /// <summary>
        /// Счет дебет
        /// </summary>
        /// <returns></returns>
        public Account DebitAccount { get; set; }

        /// <summary>
        /// Идентификатор счета кредита
        /// </summary>
        [CustomValidation(typeof(IOrder), "CreditAccountIdValidate")]
        public int? CreditAccountId { get; set; }

        /// <summary>
        /// Счет кредит
        /// </summary>
        public Account CreditAccount { get; set; }
		/// <summary>
		/// Статья расхода
		/// </summary>
		public OrderExpense OrderExpense { get; set; }

		/// <summary>
		/// Идентификатор клиента (от кого получено/кому выдано)
		/// </summary>
		[CustomValidation(typeof(CashOrder), "ClientValidate")]
        public int? ClientId { get; set; }

        /// <summary>
        /// Клиент (от кого получено/кому выдано)
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Идентификатор пользователя (от кого получено/кому выдано)
        /// </summary>
        [CustomValidation(typeof(CashOrder), "ClientValidate")]
        public int? UserId { get; set; }

        /// <summary>
        /// Пользователь (от кого получено/кому выдано)
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string ClientName => User?.Fullname ?? Client?.FullName;

        /// <summary>
        /// ИИН/БИН
        /// </summary>
        public string IdentityNumber => Client?.IdentityNumber;

        /// <summary>
        /// Вид расходов
        /// </summary>
        [CustomValidation(typeof(CashOrder), "ExpenseTypeValidation")]
        public int? ExpenseTypeId { get; set; }

        /// <summary>
        /// Вид расходов
        /// </summary>
        public ExpenseType ExpenseType { get; set; }

        /// <summary>
        /// Основание
        /// </summary>
        [Required(ErrorMessage = "Поле основание обязательно для заполнения")]
        public string Reason { get; set; }

        /// <summary>
        /// Основание на казахском
        /// </summary>
        public string? ReasonKaz { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Дата регистрации
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата регистрации обязательно для заполнения")]
        public DateTime RegDate { get; set; }

        /// <summary>
        /// Идентификатор владельца
        /// </summary>
        [RequiredId(ErrorMessage = "Поле владелец обязательно для заполнения")]
        public int OwnerId { get; set; }

        /// <summary>
        /// Филиал, в котором создан кассовый ордер
        /// </summary>
        [RequiredId(ErrorMessage = "Поле филиал обязательно для заполнения")]
        public int BranchId { get; set; }

        /// <summary>
        /// Филиал, в котором создан кассовый ордер
        /// </summary>
        public Group Branch { get; set; }

        /// <summary>
        /// Автор кассового ордера
        /// </summary>
        [RequiredId(ErrorMessage = "Поле автор обязательно для заполнения")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор кассового ордера
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// True, если кассовый ордер создан сегодня
        /// </summary>
        public bool CreatedToday => OrderDate.Date == DateTime.Now.Date;

        /// <summary>
        /// Подтвердивший кассовый ордер
        /// </summary>
        public int? ApprovedId { get; set; } = 1;

        /// <summary>
        /// Подтвердивший кассовый ордер
        /// </summary>
        public User Approved { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? ApproveDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Статус кассового ордера
        /// </summary>
        public OrderStatus ApproveStatus { get; set; } = OrderStatus.Approved;

        /// <summary>
        /// Наличие документов
        /// </summary>
        public ProveType ProveType { get; set; }

        /// <summary>
        /// Идентификатор онлайн оплаты
        /// </summary>
        public long? ProcessingId { get; set; }

        /// <summary>
        /// Тип платежной системы
        /// </summary>
        public ProcessingType? ProcessingType { get; set; }

        /// <summary>
        /// Идентификатор платежной операции
        /// </summary>
        public int? OperationId { get; set; }

        /// <summary>
        /// Количество связей
        /// </summary>
        public int? RelationCount { get; set; }

        /// <summary>
        /// ИДентификатор договора
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Идентификатор действия по договору
        /// </summary>
        public int? ContractActionId { get; set; }

        /// <summary>
        /// Идентификатор бизнес-операции
        /// </summary>
        public int? BusinessOperationId { get; set; }

        /// <summary>
        /// Идентификатор настройки бизнес-операции
        /// </summary>
        public int? BusinessOperationSettingId { get; set; }

        /// <summary>
        /// Сумма в национальной валюте
        /// </summary>
        public decimal OrderCostNC { get; set; }

        /// <summary>
        /// Идентификатор валюты
        /// </summary>
        [CustomValidation(typeof(IMultiCurrency), "CurrencyValidate")]
        public int CurrencyId { get; set; }

        /// <summary>
        /// Идентификатор сторнированного ордера
        /// </summary>
        public int? StornoId { get; set; }
        /// <summary>
        /// Id платежа в TasOnline
        /// </summary>
        public int? TasOnlinePaymentId { get; set; }
        /// <summary>
        /// Id платежа в TMF
        /// </summary>
        public string? TMFPaymentId { get; set; }

        /// <summary>
        /// Статус запроса в UKassa
        /// </summary>
        public TasOnlineRequestStatus? UKassaStatus { get; set; }

        /// <summary>
        /// Язык кассового ордера - для печати ордеров и чеков
        /// </summary>
        public Language? Language { get; set; }

        /// <summary>
        /// Идентификатор запроса сервиса Аукцион
        /// </summary>
        public Guid? AuctionRequestId { get; set; }

        [JsonIgnore]
        public BusinessOperationSetting BusinessOperationSetting { get; set; }

        public static ValidationResult ClientValidate(int? value, ValidationContext context)
        {
            var order = (CashOrder)context.ObjectInstance;

            if (!order.ClientId.HasValue && !order.UserId.HasValue)
            {
                return new ValidationResult("Одно из полей Клиент либо Пользователь обязательны для заполнения");
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ExpenseTypeValidation(int? value, ValidationContext context)
        {
            var order = (CashOrder)context.ObjectInstance;

            if (order.OrderType == OrderType.CashOut && order.UserId.HasValue && !value.HasValue)
            {
                return new ValidationResult("Вид расходов обязателен для заполнения");
            }

            return ValidationResult.Success;
        }
    }
}