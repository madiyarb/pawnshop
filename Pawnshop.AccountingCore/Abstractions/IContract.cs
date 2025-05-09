using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IContract : IEntity
    {
        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// График погашения
        /// </summary>
        public List<IPaymentScheduleItem> GetSchedule();

        /// <summary>
        /// Тип договора
        /// </summary>
        public int ContractTypeId { get; set; }

        /// <summary>
        /// Тип срочности договора
        /// </summary>
        public int PeriodTypeId { get; set; }

        /// <summary>
        /// True, если договор создан сегодня
        /// </summary>
        public bool CreatedToday => ContractDate.Date == DateTime.Now.Date;

        public int AuthorId { get; set; }

        /// <summary>
        /// Идентификатор сделки в битрикс24
        /// </summary>
        public int? CrmPaymentId { get; set; }

        /// <summary>
        /// Дата следующей оплаты
        /// </summary>
        public DateTime? NextPaymentDate { get; set; }

        /// <summary>
        /// Вид удержания процентов
        /// </summary>
        public PercentPaymentType PercentPaymentType { get; set; }

        /// <summary>
        /// Дата выкупа
        /// </summary>
        public DateTime? BuyoutDate { get; set; }
        
        /// <summary>
        /// Причина выкупа
        /// </summary>
        public int? BuyoutReasonId { get; set; }

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
        /// Дата возврата
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Первоначальная дата возврата
        /// </summary>
        public DateTime OriginalMaturityDate { get; set; }

        /// <summary>
        /// Статус договора
        /// </summary>
        public ContractStatus Status { get; set; }

        /// <summary>
        /// Стоимость процента кредита
        /// </summary>
        public decimal LoanPercentCost { get; set; }
        /// <summary>
        /// Оценочная стоимость
        /// </summary>
        public int EstimatedCost { get; set; }

        /// <summary>
        /// Ссуда
        /// </summary>
        public decimal LoanCost { get; set; }
        public decimal LoanPercent { get; set; }

        /// <summary>
        /// Срок залога (дней)
        /// </summary>
        public int LoanPeriod { get; set; }

        public bool IsOffBalance { get; set; }

        /// <summary>
        /// Исполнительная надпись
        /// </summary>
        public int? InscriptionId { get; set; }

        /// <summary>
        /// Продукт
        /// </summary>
        public int? ProductTypeId { get; set; }

        /// <summary>
        /// залочен(дискретки)
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// Идентификатор закрытого договора
        /// </summary>
        public int? ClosedParentId { get; set; }

        /// <summary>
        /// Признак рестуктуризации
        /// </summary>
        public bool IsContractRestructured { get; set; }

        public bool UsePenaltyLimit { get; set; }

        public DateTime? SignDate { get; set; }
    }
}
