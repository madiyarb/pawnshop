namespace Pawnshop.Data.Models.ApplicationsOnline.Views
{
    public sealed class ApplicationOnlineClientContactsView
    {
        /// <summary>
        /// Телефон
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        public string Surname { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        public string Patronymic { get; set; }

        /// <summary>
        /// Степень родства
        /// </summary>
        public string RelativeInfo { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string AdditionalInfo { get; set; }
    }
}
