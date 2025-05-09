using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Настройки планов счетов
    /// </summary>
    public class AccountPlanSetting : IEntity, ICreateLogged, ISoftDelete
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Настройки счета
        /// </summary>
        public int AccountSettingId { get; set; }

        /// <summary>
        /// План счета
        /// </summary>
        public int? AccountPlanId { get; set; }

        /// <summary>
        /// Тип контракта
        /// </summary>
        public int ContractTypeId { get; set; }

        /// <summary>
        /// Тип срочности
        /// </summary>
        public int PeriodTypeId { get; set; }

        /// <summary>
        /// Счет
        /// </summary>
        public int? AccountId { get; set; }

        /// <summary>
        /// Счет
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// Организация
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public int? BranchId { get; set; }
    }
}
