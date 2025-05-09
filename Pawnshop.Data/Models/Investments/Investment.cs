using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Investments
{
    /// <summary>
    /// Инвестиция
    /// </summary>
    public class Investment : IEntity
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

        /// <summary>
        /// Клиент
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Сумма инвестиций
        /// </summary>
        public int InvestmentCost { get; set; }

        /// <summary>
        /// Годовая процентная ставка
        /// </summary>
        public int InvestmentPercent { get; set; }

        /// <summary>
        /// Дата внесения
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата внесения обязательно для заполнения")]
        public DateTime InvestmentBeginDate { get; set; }

        /// <summary>
        /// Период инвестиции
        /// </summary>
        public int InvestmentPeriod { get; set; }

        /// <summary>
        /// Дата окончания
        /// </summary>
        /// <value>The investment end date.</value>
        [RequiredDate(ErrorMessage = "Поле дата окончания обязательно для заполнения")]
        public DateTime InvestmentEndDate { get; set; }

        /// <summary>
        /// Актуальная сумма инвестиций
        /// </summary>
        public int ActualCost { get; set; }

        /// <summary>
        /// Актуальный период инвестиции
        /// </summary>
        public int ActualPeriod { get; set; }

        /// <summary>
        /// Актуальная дата окончания
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле актуальная дата окончания обязательно для заполнения")]
        public DateTime ActualEndDate { get; set; }

        /// <summary>
        /// День погашения начисленных процентов
        /// </summary>
        [Range(1, 30)]
        public int RepaymentDay { get; set; }

        /// <summary>
        /// Тип погашения
        /// </summary>
        [EnumDataType(typeof(InvestmentRepaymentType), ErrorMessage = "Поле тип погашения обязательно для заполнения")]
        public InvestmentRepaymentType RepaymentType { get; set; }

        /// <summary>
        /// Сумма частичного погашения
        /// </summary>
        public int? RepaymentPartCost { get; set; }

        /// <summary>
        /// Кассовый ордер
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [EnumDataType(typeof(InvestmentStatus), ErrorMessage = "Поле статус обязательно для заполнения")]
        public InvestmentStatus Status { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата создания обязательно для заполнения")]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Филиал
        /// </summary>
        [RequiredId(ErrorMessage = "Поле филиал обязательно для заполнения")]
        public int BranchId { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public Group Branch { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        [RequiredId(ErrorMessage = "Поле автор обязательно для заполнения")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Владелец
        /// </summary>
        [RequiredId(ErrorMessage = "Поле владелец обязательно для заполнения")]
        public int OwnerId { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
