using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Investments
{
    public enum RepaymentType : short
    {
        /// <summary>
        /// Полное погашение начисленного процента
        /// </summary>
        [Display(Name = "Полное погашение начисленного процента")]
        All = 10,
        /// <summary>
        /// Частичное погашение начисленного процента
        /// </summary>
        [Display(Name = "Частичное погашение начисленного процента")]
        Part = 20,
        /// <summary>
        /// Без погашения начисленного процента
        /// </summary>
        [Display(Name = "Без погашения начисленного процента")]
        None = 30
    }
}
