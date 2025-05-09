namespace Pawnshop.Data.Models.MobileApp
{
    public enum ApplicationStatus
    {
        /// <summary>
        /// Новая заявка
        /// </summary>
        New,
        /// <summary>
        /// В обработке
        /// </summary>
        Processing,
        /// <summary>
        /// Обработанная заявка
        /// </summary>
        Done,
        /// <summary>
        /// Отклонен
        /// </summary>
        Rejected
    }
}