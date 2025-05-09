using System.Collections.Generic;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class UpdateApplicationFieldsRequest
    {
        /// <summary>
        /// параметр шины <b><u>application_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// параметр шины <b><u>fields</u></b>
        /// </summary>
        public List<FiledsViewModel> Fields { get; set; } = new List<FiledsViewModel>();

        /// <summary>
        /// параметр шины <b><u>instance_id</u></b>
        /// </summary>
        public int? InstanceId { get; set; }

        /// <summary>
        /// параметр шины <b><u>loan_to</u></b>
        /// </summary>
        public bool LoanTo { get; set; }
    }
}
