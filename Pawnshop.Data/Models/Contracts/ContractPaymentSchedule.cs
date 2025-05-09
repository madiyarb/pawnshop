using IEntity = Pawnshop.Core.IEntity;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using System.Collections.Generic;
using System;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Расписание договора
    /// </summary>
    public class ContractPaymentSchedule : IEntity, IPaymentScheduleItem
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Ожидаемая дата
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Ожидаемая дата
        /// </summary>
        public DateTime? ActualDate { get; set; }

        /// <summary>
        /// Остаток основного долга
        /// </summary>
        public decimal DebtLeft { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal DebtCost { get; set; }

        /// <summary>
        /// Процент
        /// </summary>
        public decimal PercentCost { get; set; }

        /// <summary>
        /// Штраф
        /// </summary>
        public decimal? PenaltyCost { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Идентификатор действия
        /// </summary>
        public int? ActionId { get; set; }

        /// <summary>
        /// Дата отмены платежа
        /// </summary>
        public DateTime? Canceled { get; set; }

        /// <summary>
        /// Дата пролонгации платежа
        /// </summary>
        public DateTime? Prolongated { get; set; }

        /// <summary>
        /// Статус платежа
        /// </summary>
        public ScheduleStatus Status { get; set; }

        /// <summary>
        /// Период
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Номер последней ревизии
        /// </summary>
        public int Revision { get; set; } = 1;

        public int? ActionType { get; set; }

        /// <summary>
        /// График платежей транша за дату (только для кредитной линии)
        /// </summary>
        public IList<ContractPaymentSchedule> TranchesSchedulePayments { get; set; }

        /// <summary>
        /// Дата переноса на первый рабочий день
        /// </summary>
        public DateTime? NextWorkingDate { get; set; }
    }
}
