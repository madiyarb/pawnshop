using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Crm
{
    /// <summary>
    /// Модель для выгрузки клиента в CRM
    /// </summary>
    public class CrmUploadContact
    {
        /// <summary>
        /// идентификатор контакта в CRM
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Поля выгрузки контакта в CRM
        /// </summary>
        public CrmUploadContactFields Fields = new CrmUploadContactFields();
    }

    /// <summary>
    /// Поля выгрузки контакта в CRM
    /// </summary>
    public class CrmUploadContactFields
    {
        /// <summary>
        /// Фамилия
        /// </summary>
        public string LAST_NAME { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string NAME { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string SECOND_NAME { get; set; }

        /// <summary>
        /// Адрес фактический
        /// </summary>
        public string ADDRESS { get; set; }

        /// <summary>
        /// Адрес регистрации
        /// </summary>
        public string ADDRESS_2 { get; set; }

        /// <summary>
        /// ИИН
        /// </summary>
        public string UF_CRM_1554290627253 { get; set; }

        /// <summary>
        /// Почта/Email
        /// </summary>
        public List<CrmMultifieldItem> EMAIL { get; set; }

        /// <summary>
        /// Информация о позиции договора/вид залога
        /// </summary>
        public string UF_CRM_59EC807C2F43C { get; set; }

        /// <summary>
        /// Телефонные номера
        /// </summary>
        public List<CrmMultifieldItem> PHONE { get; set; }

        /// <summary>
        /// Идентификатор клиента во фронт-базе
        /// </summary>
        public string UF_CRM_1589434063619 { get; set; }
    }
}