using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Domains
{
    /// <summary>
    /// Значение домена
    /// </summary>
    public class DomainValueDto
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Альтернативное название домена
        /// </summary>
        public string NameAlt { get; set; }
        /// <summary>
        /// Код
        /// </summary>
        [Required, RegularExpression("^[A-Z\\d_]+$", ErrorMessage = "Код должен содержаться из латинских символов в верхнем регистре, цифр или знака нижнего подчеркивания '_'")]
        public string Code { get; set; }
        /// <summary>
        /// Дополнительная информация
        /// </summary>
        public object AdditionalData { get; set; }

        [JsonIgnore]
        public string AdditionalDataSerialized
        {
            get
            {
                return AdditionalData != null ? JsonConvert.SerializeObject(AdditionalData) : null;
            }
        }

        /// <summary>
        /// Флаг активности домена
        /// </summary>
        [Required]
        public bool? IsActive { get; set; }
    }
}
