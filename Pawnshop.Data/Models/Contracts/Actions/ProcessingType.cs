using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    /// <summary>
    /// Наименование платежной системы
    /// </summary>
    public enum ProcessingType : short
    {
        /// <summary>
        /// Касса24
        /// </summary>
        [Display(Name = "Касса24")]
        Kassa24 = 10,

        /// <summary>
        /// processing.kz
        /// </summary>
        [Display(Name = "Processing.kz")]
        Processing = 20,

        /// <summary>
        /// RPS
        /// </summary>
        [Display(Name = "RPS Asia")]
        Rps = 30,

        /// <summary>
        /// Qiwi
        /// </summary>
        [Display(Name = "Qiwi")]
        Qiwi = 40,

        /// <summary>
        /// JetPay
        /// </summary>
        [Display(Name = "JetPay")]
        JetPay = 50,

        /// <summary>
        /// PayDala
        /// </summary>
        [Display(Name = "PayDala")]
        PayDala = 60,

        /// <summary>
        /// Kaspi
        /// </summary>
        [Display(Name = "Kaspi")]
        Kaspi = 70
    }
}
