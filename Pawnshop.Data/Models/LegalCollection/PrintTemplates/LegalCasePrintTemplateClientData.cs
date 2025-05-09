using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.LegalCollection.PrintTemplates
{
    public class LegalCasePrintTemplateClientData
    {
        /// <summary>
        /// Полное ФИО клиента
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Полный адресс клиента
        /// </summary>
        public string FullAddress { get; set; }
        /// <summary>
        /// Контактный номер телефона
        /// </summary>
        public string ContactPhoneNumber { get; set; }
        /// <summary>
        /// ИИН
        /// </summary>
        public string IIN { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime BirthDay { get; set; }
    }
}
