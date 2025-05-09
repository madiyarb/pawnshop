using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Тип карты клиента
    /// </summary>
    public enum CardType : short
    {
        /// <summary>
        /// Стандартная
        /// </summary>
        Standard = 10,
        /// <summary>
        /// Бронзовая
        /// </summary>
        Bronze = 20,
        /// <summary>
        /// Серебряная
        /// </summary>
        Silver = 30,
        /// <summary>
        /// Золотая
        /// </summary>
        Gold = 40,
        /// <summary>
        /// Платиновая
        /// </summary>
        Platinum = 50,
    }
}