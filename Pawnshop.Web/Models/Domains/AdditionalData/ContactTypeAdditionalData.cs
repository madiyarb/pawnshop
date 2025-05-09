using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Domains.AdditionalData
{
    public class ContactTypeAdditionalData
    {
        /// <summary>
        /// Регулярное значение для проверки контакта
        /// </summary>
        public string Regex { get; set; }
        /// <summary>
        /// Шаблон ошибки
        /// </summary>
        public string ErrorMessageTemplate { get; set; }
    }
}
