using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Расходы клиента
    /// </summary>
    public class ClientExpense
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Сумма задолжности по всем непогашенным кредитам
        /// </summary>
        public int? AllLoan { get; set; }

        /// <summary>
        /// По кредитам
        /// </summary>
        public int? Loan { get; set; }

        /// <summary>
        /// Среднемесячный платеж кредита оформленного в день расчета КДН
        /// </summary>
        public int? AvgPaymentToday { get; set; } 

        /// <summary>
        /// Другие
        /// </summary>
        public int? Other { get; set; }

        /// <summary>
        /// По дому
        /// </summary>
        public int? Housing { get; set; }

        /// <summary>
        /// По семьям
        /// </summary>
        public int? Family { get; set; }

        /// <summary>
        /// По транспорту
        /// </summary>
        public int? Vehicle { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        public List<string> EmptyFields()
        {
            var emptyFiеlds = new List<string>();

            if (!Housing.HasValue)
                emptyFiеlds.Add("Расходы на содержание жилья");
            if (!Loan.HasValue)
                emptyFiеlds.Add("Расходы по прочим кредитам");
            if (!Family.HasValue)
                emptyFiеlds.Add("Расходы на содержание семьи");
            if (!Vehicle.HasValue)
                emptyFiеlds.Add("Расходы на содержание авто");
            if (!Other.HasValue)
                emptyFiеlds.Add("Ежемесячные прочие расходы");
            if (!AllLoan.HasValue)
                emptyFiеlds.Add("Общая задолжность по всем кредитам");

            return emptyFiеlds;
        }
    }
}
