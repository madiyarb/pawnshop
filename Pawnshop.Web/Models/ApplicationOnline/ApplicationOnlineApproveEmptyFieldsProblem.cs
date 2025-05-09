using System.Collections.Generic;

namespace Pawnshop.Web.Models.ApplicationOnline
{
    public class ApplicationOnlineApproveEmptyFieldsProblem : BaseResponse
    {
        public List<string> EmptyFields { get; set; }
    }
}
