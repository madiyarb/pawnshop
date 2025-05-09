using System;

namespace Pawnshop.Web.Models.BranchesPartnerCodes
{
    public class BranchPartnerCodeUpdateBinding
    {
        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Код партнёра
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Включен/выключен
        /// </summary>
        public bool Enabled { get; set; }
    }
}
