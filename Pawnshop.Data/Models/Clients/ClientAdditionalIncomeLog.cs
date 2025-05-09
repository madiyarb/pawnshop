using System;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Логи доп доходов клиента [Архивные логи]
    /// </summary>
    [Obsolete]
    public class ClientAdditionalIncomeLog : ClientAdditionalIncome
    {
        /// <summary>
        /// Идентификатор родительской записи
        /// </summary>
        public int ClientAdditionalIncomeId { get; set; }

        /// <summary>
        /// Обновлено пользователем
        /// </summary>
        public int? UpdatedByAuthorId { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}
