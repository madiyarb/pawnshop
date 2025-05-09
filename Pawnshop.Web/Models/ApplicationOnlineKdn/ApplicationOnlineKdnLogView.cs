using System;

namespace Pawnshop.Web.Models.ApplicationOnlineKdn
{
    public class ApplicationOnlineKdnLogView
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Идентификатор автора
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// ФИО автора
        /// </summary>
        public string CreateByName { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Идентификатор заявки
        /// </summary>
        public Guid ApplicationOnlineId { get; set; }

        /// <summary>
        /// Статус заявки
        /// </summary>
        public string ApplicationOnlineStatus { get; set; }

        /// <summary>
        /// Сумма заявки
        /// </summary>
        public decimal ApplicationAmount { get; set; }

        /// <summary>
        /// Срок заявки
        /// </summary>
        public int ApplicationTerm { get; set; }

        /// <summary>
        /// Идентификатор продукта заявки
        /// </summary>
        public int ApplicationSettingId { get; set; }

        /// <summary>
        /// Итоговая сумма доходов
        /// </summary>
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// Прочие ежемесячные платежи в других ФИ
        /// </summary>
        public decimal OtherPaymentsAmount { get; set; }

        /// <summary>
        /// Среднемесячный платеж по заявке
        /// </summary>
        public decimal AverageMonthlyPayment { get; set; }

        /// <summary>
        /// Признак подтверждения дохода
        /// </summary>
        public bool IncomeConfirmed { get; set; }

        /// <summary>
        /// КДН
        /// </summary>
        public decimal Kdn { get; set; }

        /// <summary>
        /// Признак успешного прохождения расчета КДН
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Текст результата
        /// </summary>
        public string ResultText { get; set; }
    }
}
