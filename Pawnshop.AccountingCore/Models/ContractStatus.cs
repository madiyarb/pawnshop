using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.AccountingCore.Models
{
    public enum ContractStatus : short
    {
        /// <summary>
        /// Черновик
        /// </summary>
        [Display(Name = "Черновик")]
        Draft = 0,

        /// <summary>
        /// Ожидает открытия для первоначального взноса
        /// </summary>
        [Display(Name = "Ожидает открытия для ПВ")]
        AwaitForInitialFeeOpen = 4,

        /// <summary>
        /// Ожидает первоначального взноса
        /// </summary>
        [Display(Name = "Ожидает первоначального взноса")]
        AwaitForInitialFee = 5,

        /// <summary>
        /// Ожидает согласования(для недвижимости)
        /// </summary>
        [Display(Name = "Ожидает согласования")]
        AwaitForConfirmation = 7,

        /// <summary>
        /// Согласован(для недвижимости)
        /// </summary>
        [Display(Name = "Согласован")]
        Confirmed = 8,

        /// <summary>
        /// Регистрация или переоформление позиции
        /// </summary>
        [Display(Name = "Оформление или переоформление позиций")]
        PositionRegistration = 10,

        /// <summary>
        /// Позиция переоформлена
        /// </summary>
        [Display(Name = "Позиция переоформлена")]
        Reissued = 11,

        /// <summary>
        /// Регистрация или переоформление позиции
        /// </summary>
        [Display(Name = "Ожидает подписания договора")]
        AwaitForSign = 15,

        /// <summary>
        /// Ожидает зачисления денег
        /// </summary>
        [Display(Name = "Ожидает зачисления денег")]
        AwaitForMoneySend = 20,

        /// <summary>
        /// Ожидает подтверждения кассового ордера
        /// </summary>
        [Display(Name = "Ожидает подтверждения кассового ордера")]
        AwaitForOrderApprove = 24,

        /// <summary>
        /// Отменен/отклонен
        /// </summary>
        [Display(Name = "Отменен/отклонен")]
        Canceled = 25,

        /// <summary>
        /// Пересоздание заявки в СК при отмене заявки на автокредите
        /// </summary>
        [Display(Name = "Ожидает создания заявки в СК")]
        AwaitForInsuranceCopy = 26,

        /// <summary>
        /// Ожидает отправки заявки в СК
        /// </summary>
        [Display(Name = "Ожидает отправки заявки в СК")]
        AwaitForInsuranceSend = 27,

        /// <summary>
        /// Подтвержден в СК
        /// </summary>
        [Display(Name = "Подтвержден в СК")]
        InsuranceApproved = 29,

        /// <summary>
        /// Подписан
        /// </summary>
        [Display(Name = "Подписан")]
        Signed = 30,

        /// <summary>
        /// Выкуплен
        /// </summary>
        [Display(Name = "Выкуплен")]
        BoughtOut = 40,

        /// <summary>
        /// Отправлен на реализацию
        /// </summary>
        [Display(Name = "Отправлен на реализацию")]
        SoldOut = 50,

        /// <summary>
        /// Реализован
        /// </summary>
        [Display(Name = "Реализован")]
        Disposed = 60
    }
}
