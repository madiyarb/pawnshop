using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Pawnshop.Data.Models.Contracts
{
    public enum ContractDisplayStatus : short
    {
        /// <summary>
        /// Новый
        /// </summary>
        [Display(Name = "Новый")]
        New = 0,

        /// <summary>
        /// Открыт
        /// </summary>
        [Display(Name = "Открыт")]
        AwaitForMoneySend = 5,

        /// <summary>
        /// Открыт
        /// </summary>
        [Display(Name = "Открыт")]
        Open = 10,

        /// <summary>
        /// Просрочен
        /// </summary>
        [Display(Name = "Просрочен")]
        Overdue = 20,


        /// <summary>
        /// Софт Коллекшн
        /// </summary>
        [Display(Name = "Софт Коллекшн")]
        SoftCollection = 21,


        /// <summary>
        /// Хард Коллекшн
        /// </summary>
        [Display(Name = "Хард Коллекшн")]
        HardCollection = 22,

        /// <summary>
        /// Легал Коллекшн
        /// </summary>
        [Display(Name = "Легал Коллекшн")]
        LegalCollection = 23,

        /// <summary>
        /// Легал + Хард Коллекшн
        /// </summary>
        [Display(Name = "Легал и Хард Коллекшн")]
        LegalHardCollection = 24,

        /// <summary>
        /// Продлен
        /// </summary>
        [Display(Name = "Продлен")]
        Prolong = 30,

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
        SoldOuted = 55,

        /// <summary>
        /// Удален
        /// </summary>
        [Display(Name = "Удален")]
        Deleted = 60,

        /// <summary>
        /// Действующие
        /// </summary>
        [Display(Name = "Действующие")]
        Signed = 70,

        /// <summary>
        /// Передано
        /// </summary>
        [Display(Name = "Передано")]
        Transfered = 80,

        /// <summary>
        /// Поступила ежемесячная оплата(погашались)
        /// </summary>
        [Display(Name = "Ежемесячное погашение")]
        MonthlyPayment = 90,

    }
}