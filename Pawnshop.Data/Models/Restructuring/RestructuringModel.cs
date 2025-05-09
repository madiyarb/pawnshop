using Microsoft.AspNetCore.Http;
using System;

namespace Pawnshop.Data.Models.Restructuring
{
    public class RestructuringModel
    {
        /// <summary>
        /// идентификатор контракта
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// количество месяцев для отсрочки
        /// </summary>
        public int? DefermentMonthCount { get; set; }

        /// <summary>
        /// фактическая дата начала отсрочки
        /// </summary>
        public DateTime StartDefermentDate { get; set; }

        /// <summary>
        /// фактическая дата конца отсрочки
        /// </summary>
        public DateTime EndDefermentDate { get; set; }

        /// <summary>
        /// количество месяцев после отсрочки (чтобы можно было растянуть для того чтобы ГЭСВ не превышал 56%)
        /// </summary>
        public int? RestructuredMonthCount { get; set; }

        /// <summary>
        /// тип отсрочки
        /// </summary>
        public int? DefermentTypeId { get; set; }

        /// <summary>
        /// документ поддверждающий отсрочку
        /// </summary>
        public IFormFile? DocumentFile { get; set; }
    }
}

