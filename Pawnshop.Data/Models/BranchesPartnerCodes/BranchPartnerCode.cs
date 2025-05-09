using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.BranchesPartnerCodes
{
    [Table("BranchesPartnerCodes")]
    public sealed class BranchesPartnerCode
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public int BranchId { get; set; }

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

        public BranchesPartnerCode()
        {
            
        }

        public BranchesPartnerCode(Guid id, int branchId, string partnerCode)
        {
            Id = id;
            BranchId = branchId;
            PartnerCode = partnerCode;
            CreateDate = DateTime.Now;
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }

        public void Enable()
        {
            Enabled = true;
        }
        public void Delete()
        {
            DeleteDate = DateTime.Now;
        }

        public void Update(int? branchId, string partnerCode, DateTime? deleteDate, bool? enabled)
        {
            if (branchId != null)
            {
                BranchId = branchId.Value;
            }

            if (!string.IsNullOrEmpty(partnerCode))
            {
                PartnerCode = partnerCode;
            }

            if (deleteDate != null)
            {
                DeleteDate = DateTime.Now;
            }

            if (enabled != null)
            {
                Enabled = enabled.Value;
            }
        }
    }
}
