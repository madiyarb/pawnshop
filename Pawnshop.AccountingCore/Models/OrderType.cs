using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Тип ордера
    /// </summary>
    public enum OrderType : short
    {
        /// <summary>
        /// Кассовый приход
        /// </summary>
        [Display(Name = "Приходный кассовый ордер", ShortName = "ПКО")]
        CashIn = 10,
        /// <summary>
        /// Кассовый расход
        /// </summary>
        [Display(Name = "Расходный кассовый ордер", ShortName = "РКО")]
        CashOut = 20,
        /// <summary>
        /// Мемориальный
        /// </summary>
        [Display(Name = "Мемориальный ордер", ShortName = "МЕМ")]
        Memorial = 30,
        /// <summary>
        /// Приходный внебалансовый ордер
        /// </summary>
        [Display(Name = "Приходный внебалансовый ордер", ShortName = "ПВО")]
        OffBalanceIn = 40,
        /// <summary>
        /// Расходный внебалансовый ордер
        /// </summary>
        [Display(Name = "Расходный внебалансовый ордер", ShortName = "РВО")]
        OffBalanceOut = 50,
        /// <summary>
        /// Платежный ордер
        /// </summary>
        [Display(Name = "Платежный ордер", ShortName = "ПО")]
        Payment = 60
    }
}