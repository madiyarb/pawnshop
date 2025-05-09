using Microsoft.AspNetCore.Mvc;
using System;

namespace Pawnshop.Web.Models.Pbx
{
    public class SaveStatusRequest
    {
        [FromQuery(Name = "num")]
        public string PhoneNumber { get; set; }

        [FromQuery(Name = "uq")]
        public string CallPbxId { get; set; }

        [FromQuery(Name = "status")]
        public string Status { get; set; }

        [FromQuery(Name = "calldate")]
        public DateTime CallDate { get; set; }

        [FromQuery(Name = "duration")]
        public string Duration { get; set; }

        [FromQuery(Name = "direction")]
        public string Direction { get; set; }

        [FromQuery(Name = "lang")]
        public string Language { get; set; }

        [FromQuery(Name = "record_file")]
        public string RecordFile { get; set; }

        [FromQuery(Name = "dnid")]
        public string UserInternalPhone { get; set; }

        [FromQuery(Name = "did")]
        public string CompanyInternalPhone { get; set; }
    }
}
