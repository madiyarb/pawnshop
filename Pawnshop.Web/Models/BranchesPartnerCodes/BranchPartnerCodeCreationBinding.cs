using System;

namespace Pawnshop.Web.Models.BranchesPartnerCodes
{
    public class BranchPartnerCodeCreationBinding
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Код партнёра
        /// </summary>
        public string PartnerCode { get; set; }
    }
}
