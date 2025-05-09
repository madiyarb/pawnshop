using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Данные контрактов, отправленных в КБ
    /// </summary>
    /// 
    public class CBContract : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор пакета в БД
        /// </summary>
        [Required(ErrorMessage = "Поле Идентификатор пакета обязательно для заполнения")]
        public int CBBatchId { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        [Required(ErrorMessage = "Поле Идентификатор кредитного договора обязательно для заполнения")]
        public int ContractId { get; set; }

        /// <summary>
        /// Код контракта
        /// </summary>
        public string ContractCode { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string AgreementNumber { get; set; }

        /// <summary>
        /// Вид финансирования
        /// </summary>
        public int FundingType { get; set; }

        /// <summary>
        /// Вид финансирования
        /// </summary>
        public string FundingSource { get; set; }

        /// <summary>
        /// Цель кредитования
        /// </summary>
        public string CreditPurpose { get; set; }

        /// <summary>
        /// Объект кредитования
        /// </summary>
        public string CreditObject { get; set; }

        /// <summary>
        /// Фаза контракта
        /// </summary>
        public int ContractPhase { get; set; }

        /// <summary>
        /// Статус контракта
        /// </summary>
        public int ContractStatus { get; set; }

        /// <summary>
        /// Наименование цессионария/правоприемника
        /// </summary>
        public string ThirdPartyHolder { get; set; }

        /// <summary>
        /// Дата начала срока действия контракта
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Дата окончания срока действия контракта
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Дата фактической выдачи
        /// </summary>
        public DateTime? ActualDate { get; set; }

        /// <summary>
        /// Период доступности кредитной линии
        /// </summary>
        public DateTime? AvailableDate { get; set; }

        /// <summary>
        /// Дата фактического завершения
        /// </summary>
        public DateTime? RealPaymentDate { get; set; }

        /// <summary>
        /// Признак связанности с Банком особыми отношениями
        /// </summary>
        public int SpecialRelationship { get; set; }

        /// <summary>
        /// Классификация контракта
        /// </summary>
        public int Classification { get; set; }

        /// <summary>
        /// Код родительского контракта
        /// </summary>
        public string ParentContractCode { get; set; }

        /// <summary>
        /// Поставщик родительского контракта
        /// </summary>
        public int? ParentProvider { get; set; }

        /// <summary>
        /// Статус родительского контракта
        /// </summary>
        public int? ParentContractStatus { get; set; }

        /// <summary>
        /// Дата проведения операции
        /// </summary>
        public DateTime? ParentOperationDate { get; set; }

        /// <summary>
        /// Количество пролонгаций
        /// </summary>
        public int? ProlongationCount { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public List<ICBSubject> Subjects { get; set; }

        public List<CBCollateral> Collaterals { get; set; }

        public CBInstallment Installment { get; set; }

        /// <summary>
        /// Вид операции в ГКБ
        /// </summary>
        public int? OperationId { get; set; }
        /// <summary>
        /// Годовая эффективная ставка
        /// </summary>
        public decimal? AnnualEffectiveRate { get; set; }
        /// <summary>
        /// Номинальная ставка
        /// </summary>
        public decimal? NominalRate { get; set; }
    }
}