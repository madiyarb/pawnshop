using System.Collections.Generic;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class CrmStateViewModel
    {
        /// <summary>
        /// Параметр шины <b><u>application_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>services</u></b>
        /// </summary>
        public List<ServiceViewModel> Services { get; set; }

        /// <summary>
        /// Параметр шины <b><u>from</u></b>
        /// </summary>
        public string From { get; set; }
    }
}
