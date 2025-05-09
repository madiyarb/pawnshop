using Pawnshop.Core;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Models.ClientLogItems
{
    public class ClientLogData
    {
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

        public ClientLogData()
        {
            
        }

        public ClientLogData(Client client)
        {
            ClientId = client.Id;
            Surname = client.Surname;
            Name = client.Name;
            Patronymic = client.Patronymic;
            BirthDay = client.BirthDay;
            IdentityNumber = client.IdentityNumber;
        }
    }
}
