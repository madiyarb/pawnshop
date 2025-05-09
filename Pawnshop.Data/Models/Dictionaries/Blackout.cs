using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Настройки выключения начисления процентов или штрафов
    /// </summary>
    public class Blackout : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// Дата конца
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Процент начисления вознаграждения
        /// </summary>
        public decimal? LoanPercent { get; set; }

        /// <summary>
        /// Коэффициент начисления вознаграждения
        /// </summary>
        public decimal LoanCoefficient => LoanPercent.HasValue ? (LoanPercent.Value / 100) : 1;

        /// <summary>
        /// Процент начисления штрафа
        /// </summary>
        public decimal? PenaltyPercent { get; set; }

        /// <summary>
        /// Коэффициент начисления штрафа
        /// </summary>
        public decimal PenaltyCoefficient => LoanPercent.HasValue ? (LoanPercent.Value / 100) : 1;

        /// <summary>
        /// Персональная скидка
        /// </summary>
        public bool IsPersonal { get; set; }
    }
}
