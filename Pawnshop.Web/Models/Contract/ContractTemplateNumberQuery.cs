using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Contract
{
    public class ContractTemplateNumberQuery
    {
        /// <summary>
        /// Шаблон печатной формы
        /// </summary>
        public int TemplateId { get; set; }
        /// <summary>
        /// Договор
        /// </summary>
        public int ContractId { get; set; }

        public bool? HasCoBorrower { get; set; }
    }
}
