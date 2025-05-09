namespace Pawnshop.Data.Models.Contracts.Actions
{
    public enum ContractPositionStatus
    {
        /// <summary>
        /// Активный
        /// </summary>
        Active,

        /// <summary>
        /// Выкуплен
        /// </summary>
        BoughtOut,

        /// <summary>
        /// Перенесен в новый договор
        /// </summary>
        PulledOut,

        /// <summary>
        /// Отправлен на реализацию
        /// </summary>
        SoldOut,

        /// <summary>
        /// Реализован
        /// </summary>
        Disposed
    }
}