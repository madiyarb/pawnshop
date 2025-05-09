using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class ClientContactType : IDictionary
    {
        public int Id { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Название на казахском
        /// </summary>
        public string NameKaz { get; set; }
        

        public string Code { get; set; }
        /// <summary>
        /// Признак доступности
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Регулярное выражение для проверки значения контакта
        /// </summary>
        public string ValueMask { get; set; }
        public string ValueMaskError { get; set; }
    }
}
