using Microsoft.AspNetCore.Mvc;

namespace Pawnshop.Web.Models.Pbx
{
    public class IncomingRequest
    {
        [FromQuery(Name = "num")]
        public string PhoneNumber { get; set; }

        [FromQuery(Name = "uq")]
        public string CallPbxId { get; set; }

        [FromQuery(Name = "dnid")]
        public string UserInternalPhone { get; set; }

        [FromQuery(Name = "lang")]
        public string Language { get; set; }
    }
}
