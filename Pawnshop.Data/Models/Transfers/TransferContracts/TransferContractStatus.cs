using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Transfers.TransferContracts
{
    public enum TransferContractStatus : short
    {
        /// <summary>
        /// Новый
        /// </summary>
        [Display(Name = "Новый")]
        New = 0,

        /// <summary>
        /// Успешно
        /// </summary>
        [Display(Name = "Выполнено")]
        Success = 10,

        /// <summary>
        /// Неудача
        /// </summary>
        [Display(Name = "Не выполнено")]
        Fail = 20,
    }
}