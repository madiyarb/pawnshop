using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// Клиент
    /// </summary>
    public class MintosClient
    {
        /// <param name="client">Клиент</param>
        public MintosClient(Client client, string defaultContact)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (!client.LegalForm.IsIndividual) throw new PawnshopApplicationException("Клиент не является физ лицом");
            if (client.IsPolitician) throw new PawnshopApplicationException("Клиент является politically exposed person (PEP)");
            id = client.Id;
            gender = (bool)client.IsMale ? "m" : "f";
            age = DateTime.Today.Year - client.BirthDay.Value.Year;
            if (client.BirthDay.Value > DateTime.Today.AddYears(-age)) age--;
            personal_identification = client.IdentityNumber;
            phone_number = defaultContact;
            address_street_actual = client.Addresses.FirstOrDefault(x=>x.IsActual && x.AddressType.Code.Contains("REGISTRATION"))?.FullPathRus;
            name = client.Name;
            surname = client.Surname;
            email = client.Email;
        }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Пол
        /// </summary>
        public string gender { get; set; }

        /// <summary>
        /// Возраст
        /// </summary>
        public int age { get; set; }

        /// <summary>
        /// ИИН/БИН
        /// </summary>
        public string personal_identification { get; set; }

        /// <summary>
        /// Номер телефона
        /// </summary>
        public string phone_number { get; set; }

        /// <summary>
        /// Факт.адрес проживания
        /// </summary>
        public string address_street_actual { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        public string surname { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// Признак 1-физ/2-юр.лицо
        /// </summary>
        public int legal_type => 1;//по умолчанию - индивидуальный(физ.лицо)
    }
}
