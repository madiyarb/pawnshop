using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Domains
{
    /// <summary>
    /// Домен
    /// </summary>
    public class DomainDto
    {
        /// <summary>
        /// Код
        /// </summary>
        [Required, RegularExpression("^[A-Z_]+$")]
        public string Code { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Альтернативное название
        /// </summary>
        public string NameAlt { get; set; }
    }
}
