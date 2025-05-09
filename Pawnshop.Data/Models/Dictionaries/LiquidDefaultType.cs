using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public enum LiquidDefaultType : short
    {
        /// <summary>
        /// Нет
        /// </summary>
        [Display(Name = "Нет")]
        No = 0,
        /// <summary>
        /// Низкий
        /// </summary>
        [Display(Name = "Низкий")]
        Low = 1,
        /// <summary>
        /// Средний
        /// </summary>
        [Display(Name = "Средний")]
        Middle = 2,
        /// <summary>
        /// Высокий
        /// </summary>
        [Display(Name = "Высокий")]
        High = 3
    }
}
