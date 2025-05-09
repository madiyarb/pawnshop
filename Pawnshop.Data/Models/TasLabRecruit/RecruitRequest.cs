using System;

namespace Pawnshop.Data.Models.TasLabRecruit
{
    public class RecruitRequest
    {
        public int Id { get; set; }

        /// <summary>
        /// 1 - ByIIN
        /// 2 - Delta (используется индекс из List метода)
        /// 3 - List (для всех военнослужащих клиентов компании)
        /// </summary>
        public RecruitRequestType RequestType { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// Метод презназначен для получения для получения данных, которые изменились/появились между датами запроса метода list или delta с указанием индекса данных.
        /// запрос Delta
        /// </summary>
        public long? RequestIndex { get; set; }

        /// <summary>
        /// Метод предназначен для получения статуса по ИИН
        /// </summary>
        public string RequestIIN { get; set; }

        /// <summary>
        /// Получение списка призывников, у кого есть действующие контракты у провайдера
        /// ответ List, число по которому можно запустить запрос Delta
        /// </summary>
        public int? ResponseIndex { get; set; }

        /// <summary>
        /// сообщение об ошибке
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Кредитное бюро (источник данных)
        /// </summary>
        public int CBType { get; set; }

        /// <summary>
        /// Ответ из КБ
        /// </summary>
        public string ResponseJson { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? DeleteDate { get; set; }
    }
}
