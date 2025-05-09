using System;

namespace Pawnshop.Web.Models.Processing
{
    /// <summary>
    /// Внешняя информация для платежных систем
    /// </summary>
    public class OnlinePaymentOuterModel
    {
        /// <summary>
        /// Код ответа
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Дата и время операции
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Номер операции (транзакции)
        /// </summary>
        public int? Authcode { get; set; }

        /// <summary>
        /// Вывод дополнительной информации
        /// </summary>
        public dynamic invoice { get; set; }


    }
}
