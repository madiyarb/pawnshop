using Microsoft.AspNetCore.Mvc;

namespace Pawnshop.Web.Models.Page
{
    public class PageSettingFromQuery
    {
        [FromQuery(Name = "offset")]
        public int? Offset { get; set; }

        [FromQuery(Name = "limit")]
        public int? Limit { get; set; }
    }
}
