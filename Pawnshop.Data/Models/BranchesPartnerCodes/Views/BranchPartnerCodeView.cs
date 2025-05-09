using System;

namespace Pawnshop.Data.Models.BranchesPartnerCodes.Views
{
    public sealed class BranchPartnerCodeView
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Название филиала 
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Код партнёра
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

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
