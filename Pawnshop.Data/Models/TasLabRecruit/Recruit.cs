using System;

namespace Pawnshop.Data.Models.TasLabRecruit
{
    public class Recruit
    {
        /// <summary>
        /// ИИН призывника
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Статус призывника:
        /// •	true – призван на срочную воинскую службу
        /// •	false – уволен со срочной воинской службы.
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// Дата призыва или увольнения. Зависит от статуса.
        /// </summary>
        public DateTime Date { get; set; }
    }
}
