using Microsoft.AspNetCore.Mvc;

namespace Pawnshop.Web.Models.Pbx
{
    public class OutgoingRequest
    {
        [FromQuery(Name = "num")]
        public string PhoneNumber { get; set; }

        [FromQuery(Name = "uq")]
        public string CallPbxId { get; set; }

        [FromQuery(Name = "dnid")]
        public string UserInternalPhone { get; set; }
    }
}
