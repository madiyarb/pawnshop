using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.ApplicationsOnline.Kdn
{
    public class ApplicationOnlineKdnLog : IEntity
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
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Идентификатор автора
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        [JsonIgnore]
        public User Author { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        [JsonIgnore]
        public Client Client { get; set; }

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

        /// <summary>
        /// Сумма доходов для сравнения
        /// </summary>
        public decimal CompareIncomeAmount { get; set; }

        /// <summary>
        /// Сумма расходов для сравнения
        /// </summary>
        public decimal CompareExpensesAmount { get; set; }

        /// <summary>
        /// Признак лудомана клиента
        /// </summary>
        public bool IsGambler { get; set; }

        /// <summary>
        /// Признак Stop Credit клиента
        /// </summary>
        public bool IsStopCredit { get; set; }

        /// <summary>
        /// Сумма платежа кредитов оформленных в день проверки КДН
        /// </summary>
        public decimal AvgPaymentToday { get; set; }

        /// <summary>
        /// Сумма задолжности по всем кредитам клиентам во всех фин. организациях
        /// </summary>
        public decimal? AllLoan { get; set; }

        /// <summary>
        /// Годовая сумма доходов
        /// </summary>
        public decimal TotalIncomeK4 { get; set; }

        /// <summary>
        /// Расчитанный коэфицент К4
        /// </summary>
        public decimal KdnK4 { get; set; }
    }
}
