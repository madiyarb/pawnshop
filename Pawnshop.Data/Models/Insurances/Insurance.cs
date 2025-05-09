using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Insurances
{
    /// <summary>
    /// Страховой договор
    /// </summary>
    public class Insurance : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        [RequiredId(ErrorMessage = "Поле договор обязательно для заполнения")]
        public int ContractId { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        public Contract Contract { get; set; }

        /// <summary>
        /// Номер страхового договора
        /// </summary>
        public string InsuranceNumber { get; set; }

        /// <summary>
        /// Сумма страховки
        /// </summary>
        public int InsuranceCost { get; set; }

        /// <summary>
        /// Период страхования
        /// </summary>
        public int InsurancePeriod { get; set; }

        /// <summary>
        /// Дата оформления
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата оформления обязательно для заполнения")]
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// Дата закрытия
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Сумма к возврату
        /// </summary>
        public int? CashbackCost { get; set; }

        /// <summary>
        /// Предыдущий страховой договор
        /// </summary>
        public int? PrevInsuranceId { get; set; }

        /// <summary>
        /// Данные страхового договора
        /// </summary>
        public InsuranceData InsuranceData { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [EnumDataType(typeof(InsuranceStatus), ErrorMessage = "Поле статус обязательно для заполнения")]
        public InsuranceStatus Status { get; set; } = InsuranceStatus.Draft;

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
        /// Пользователь
        /// </summary>
        [RequiredId(ErrorMessage = "Поле пользователь обязательно для заполнения")]
        public int UserId { get; set; }

        /// <summary>
        /// Владелец
        /// </summary>
        [RequiredId(ErrorMessage = "Поле владелец обязательно для заполнения")]
        public int OwnerId { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Действия
        /// </summary>
        public List<InsuranceAction> Actions { get; set; } = new List<InsuranceAction>();
    }
}
