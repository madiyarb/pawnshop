using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.TasLabRecruit
{
    public class RecruitIINResponse
    {
        public string IIN { get; set; }

        /// <summary>
        /// Статус призывника:
        ///•	true – призван на срочную воинскую службу
        ///•	false – уволен со срочной воинской службы.
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// Дата призыва или увольнения. Зависит от статуса.
        /// </summary>
        public DateTime Date { get; set; }

        public string Message { get; set; }
    }
}
