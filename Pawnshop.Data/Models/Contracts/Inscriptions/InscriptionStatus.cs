using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Contracts.Inscriptions
{
    /// <summary>
    /// Статус исполнительной надписи
    /// </summary>
    public enum InscriptionStatus : short
    {
        /// <summary>
        /// Новая/Рассматривается
        /// </summary>
        [Display(Name = "Новая")]
        New = 0,

        /// <summary>
        /// Подтверждена
        /// </summary>
        [Display(Name = "Подтверждена")]
        Approved = 10,

        /// <summary>
        /// Отозвана/отменена
        /// </summary>
        [Display(Name = "Отозвана")]
        Denied = 20,

        /// <summary>
        /// Исполнена
        /// </summary>
        [Display(Name = "Исполнена")]
        Executed = 30
    }
}