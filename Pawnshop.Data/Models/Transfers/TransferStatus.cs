using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Transfers
{
    public enum TransferStatus : short
    {
        /// <summary>
        /// Черновик
        /// </summary>
        [Display(Name = "Черновик")]
        Draft = 0,

        /// <summary>
        /// Успешно
        /// </summary>
        [Display(Name = "Успешно")]
        Success = 10
    }
}