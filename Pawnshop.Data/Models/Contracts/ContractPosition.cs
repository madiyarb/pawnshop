using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries;
using System;
using Pawnshop.Data.Models.Contracts.Actions;
using System.Collections.Generic;
using Pawnshop.Data.Models.Positions;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Позиция договора
    /// </summary>
    public class ContractPosition : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        public Contract Contract { get; set; }

        /// <summary>
        /// Позиция
        /// </summary>
        [RequiredId(ErrorMessage = "Поле позиция обязательно для заполнения")]
        public int PositionId { get; set; }

        /// <summary>
        /// Позиция
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        public int PositionCount { get; set; }

        /// <summary>
        /// Оценка
        /// </summary>
        public int EstimatedCost { get; set; }

        /// <summary>
        /// Требуемый первоначальный взнос
        /// </summary>
        public decimal RequiredInitialFee { get; set; }

        /// <summary>
        /// Минимальный первоначальный взнос(по продукту)
        /// </summary>
        public decimal MinimalInitialFee { get; set; }

        /// <summary>
        /// Ссуда
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Категория аналитики
        /// </summary>
        [RequiredId(ErrorMessage = "Поле категория аналитики обязательно для заполнения")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Категория аналитики
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Специфичные поля позиции договора
        /// </summary>
        public GoldContractSpecific PositionSpecific { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Статус позиции
        /// </summary>
        public ContractPositionStatus Status { get; set; }

        ///<summary>
        /// Оценка залога
        /// </summary>
        public PositionEstimate? PositionEstimate { get; set; }

        ///<summary>
        /// Идентицифкатор оценки залога
        /// </summary>
        public int? EstimationId { get; set; }

        ///<summary>
        /// Залоговая стоимость
        /// </summary>
        public decimal? CollateralCost { get; set; }

        /// <summary>
        /// Номер договора залога
        /// </summary>
        public string? PositionContractNumber { get; set; }

        /// <summary>
        /// История оценок
        /// </summary>
        public List<PositionEstimateHistory> PositionEstimateHistory { get; set; }
        /// <summary>
        /// Сумма motor лимита
        /// </summary>
        public int? MotorCost { get; set; }
        /// <summary>
        /// Сумма turbo лимита
        /// </summary>
        public int? TurboCost { get; set; }
    }
}
