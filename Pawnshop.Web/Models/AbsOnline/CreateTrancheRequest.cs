using System;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class CreateTrancheRequest : CreateApplicationRequest
    {
        /// <summary>
        /// Параметр шины <b><u>contract_base</u></b> (номер родительского займа)
        /// </summary>
        public string BaseContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>date_end</u></b> (дата окончания)
        /// </summary>
        public DateTime MaturityDate { get; set; }
    }
}
