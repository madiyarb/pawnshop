using System;

namespace Pawnshop.Data.Models.BranchesPartnerCodes.Query
{
    public sealed class BranchPartnerCodeListQuery
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Код партнёра
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Включен/выключен
        /// </summary>
        public bool? Enabled { get; set; }
    }
}
