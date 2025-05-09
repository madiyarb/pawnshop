using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Тип причины постановки в черный список
    /// </summary>
    public enum ReasonType : short
    {
        /// <summary>
        /// Клиент
        /// </summary>
        [Display(Name = "Клиент")]
        Client = 10,
        /// <summary>
        /// Автомобиль
        /// </summary>
        [Display(Name = "Автомобиль")]
        Car = 20,
    }
}