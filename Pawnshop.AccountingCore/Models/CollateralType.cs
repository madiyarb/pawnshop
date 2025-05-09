using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.AccountingCore.Models
{

    /// <summary>
    /// Вид залога
    /// </summary>
    public enum CollateralType : short
    {
        /// <summary>
        /// Золото
        /// </summary>
        [Display(Name = "Золото")]
        Gold = 10,
        /// <summary>
        /// Автомобиль
        /// </summary>
        [Display(Name = "Автомобиль")]
        Car = 20,
        /// <summary>
        /// Товар
        /// </summary>
        [Display(Name = "Товар")]
        Goods = 30,
        /// <summary>
        /// Товар
        /// </summary>
        [Display(Name = "Спецтехника")]
        Machinery = 40,
        /// <summary>
        /// Беззалоговый(для сотрудников)
        /// </summary>
        [Display(Name = "Беззалоговый")]
        Unsecured = 50,
        /// <summary>
        /// Недвижимость
        /// </summary>
        [Display(Name = "Недвижимость")]
        Realty = 60
    }
}
