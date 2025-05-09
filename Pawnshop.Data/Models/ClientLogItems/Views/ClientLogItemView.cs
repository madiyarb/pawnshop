using System;
using System.ComponentModel;

namespace Pawnshop.Data.Models.ClientLogItems.Views
{
    public class ClientLogItemView
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public int ClientId { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        [DisplayName("Фамилия")]
        public string Surname { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        [DisplayName("Имя")]
        public string Name { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        [DisplayName("Отчество")]
        public string Patronymic { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        [DisplayName("Дата рождения/регистрации")]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// ИИН/БИН
        /// </summary>
        [DisplayName("ИИН/БИН клиента")]
        public string IdentityNumber { get; set; }
    }
}
