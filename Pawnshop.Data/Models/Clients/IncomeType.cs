using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    public enum IncomeType : short
    {
        /// <summary>
        /// Удостоверение личности
        /// </summary>
        [Display(Name = "Официальный")]
        Formal = 10,
        /// <summary>
        /// Паспорт
        /// </summary>
        [Display(Name = "Неофициальный")]
        Informal = 20
    }
}
